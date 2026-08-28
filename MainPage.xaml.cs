using System;
using System.Collections.Generic;
// https://stackoverflow.com/a/32923221/433626
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Windows.Devices.Input;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Xslt2Game
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string AuthorityLocal = "192.168.2.100:8080";// "mansoft.nl:8080"
        private const string AuthorityMansoft = "mansoft.nl:8080";
        private const string Authority = AuthorityMansoft;
        private const string ChainPath = "/XSLT2/chainreaction/chainlevel1.xml?xslt=/xslt/chainreaction-xaml.xslt";
        private const string SpiroPath = "/XSLT2/spirograaf/fig1.xml?xslt=/xslt/spiroxaml.xslt";
        private const string Path = ChainPath;
        public string Url { get; set; } = "http://" + Authority + Path;
        private ResourceDictionary model;
        private FrameworkElement game;
        private MessageWebSocket messageWebSocket;
        private DataWriter messageWriter;
        private bool busy;

        private void AppendOutputLine(string value)
        {
            Debug.WriteLine(value);
        }

        public static string BuildWebSocketError(Exception ex)
        {
            ex = ex.GetBaseException();

            if ((uint)ex.HResult == 0x800C000EU)
            {
                // INET_E_SECURITY_PROBLEM - our custom certificate validator rejected the request.
                return "Error: Rejected by custom certificate validation.";
            }

            WebErrorStatus status = WebSocketError.GetStatus(ex.HResult);

            // Normally we'd use the HResult and status to test for specific conditions we want to handle.
            // In this sample, we'll just output them for demonstration purposes.
            switch (status)
            {
                case WebErrorStatus.CannotConnect:
                case WebErrorStatus.NotFound:
                case WebErrorStatus.RequestTimeout:
                    return "Cannot connect to the server. Please make sure " +
                        "to run the server setup script before running the sample.";

                case WebErrorStatus.Unknown:
                    return "COM error: " + ex.HResult;

                default:
                    return "Error: " + status;
            }
        }

        private void CloseSocket()
        {
            if (messageWriter != null)
            {
                // In order to reuse the socket with another DataWriter, the socket's output stream needs to be detached.
                // Otherwise, the DataWriter's destructor will automatically close the stream and all subsequent I/O operations
                // invoked on the socket's output stream will fail with ObjectDisposedException.
                //
                // This is only added for completeness, as this sample closes the socket in the very next code block.
                messageWriter.DetachStream();
                messageWriter.Dispose();
                messageWriter = null;
            }

            if (messageWebSocket != null)
            {
                try
                {
                    messageWebSocket.Close(1000, "Closed due to user request.");
                }
                catch (Exception ex)
                {
                    AppendOutputLine(MainPage.BuildWebSocketError(ex));
                    AppendOutputLine(ex.Message);
                }
                messageWebSocket = null;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            CloseSocket();
        }

        async void SendToWebsocket(string message)
        {
            SetBusy(true);
            await SendAsync(message);
            SetBusy(false);
        }

        public Brush ColorToBrush(string color)
        {
            // https://stackoverflow.com/a/35215401/433626
            Windows.UI.Color c = (Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), color);
            return new SolidColorBrush(c);
        }

        public void SetXamlAttribute(string Name, string Property, string Value)
        {
            Control c = game.FindName(Name) as Control;

            if (c != null)
            {
                var prop = c.GetType().GetProperty(Property);

                if (prop != null && typeof(Brush).IsAssignableFrom(prop.PropertyType))
                {
                    prop.SetValue(c, ColorToBrush(Value));
                }
            }
        }

        public void SetModel(string path, string value)
        {
            string[] split = path.Split(new Char[] { '/' });
            int i = 0;
            string component = null;
            object Element = null;
            ResourceDictionary dictionary = model;
            while (i < split.Length)
            {
                component = split[i];
                Element = dictionary[component];
                if (Element is ResourceDictionary)
                {
                    dictionary = (ResourceDictionary)Element;
                }
                i++;
            }
            if (Element is string)
            {
                dictionary[component] = value;
            }
        }

        public void HandleCommand(XmlDocument doc)
        {
            XmlElement commands = doc.DocumentElement;
            string message = " ";
            NotifyType notifyType = NotifyType.StatusMessage;

            if (commands.LocalName == "commands")
            {
                foreach (XmlElement command in commands.ChildNodes)
                {
                    if (command.LocalName == "command")
                    {
                        switch (command.GetAttribute("type"))
                        {
                            case "update-element":
                                foreach (XmlElement child in command.ChildNodes)
                                {
                                    switch (child.LocalName)
                                    {
                                        case "xaml":
                                            SetXamlAttribute(child.GetAttribute("name"), child.GetAttribute("attribute"), child.GetAttribute("value"));
                                            break;
                                        case "model":
                                            SetModel(child.GetAttribute("path"), child.GetAttribute("value"));
                                            break;
                                    }
                                }
                                break;
                            case "show-confirm-dialog":
                                if (command.GetAttribute("message-type") == "1")
                                {
                                    notifyType = NotifyType.ErrorMessage;
                                }
                                message = command.GetAttribute("message");
                                break;
                        }
                    }
                }
            }
            NotifyUser(message, notifyType);
        }

        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };

        /// <summary>
        /// Display a message to the user.
        /// This method may be called from any thread.
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
        {
            // If called from the UI thread, then update immediately.
            // Otherwise, schedule a task on the UI thread to perform the update.
            if (Dispatcher.HasThreadAccess)
            {
                UpdateStatus(strMessage, type);
            }
            else
            {
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => UpdateStatus(strMessage, type));
            }
        }

        private void UpdateStatus(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }

            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            StatusBorder.Visibility = (StatusBlock.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
            if (StatusBlock.Text != String.Empty)
            {
                StatusBorder.Visibility = Visibility.Visible;
                StatusPanel.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Visibility.Collapsed;
                StatusPanel.Visibility = Visibility.Collapsed;
            }

            // Raise an event if necessary to enable a screen reader to announce the status update.
            var peer = FrameworkElementAutomationPeer.FromElement(StatusBlock);
            if (peer != null)
            {
                peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
            }
        }

        async Task SendAsync(string message)
        {
            if (String.IsNullOrEmpty(message))
            {
                NotifyUser("Please specify text to send", NotifyType.ErrorMessage);
                return;
            }

            AppendOutputLine("Sending Message:\n" + message);

            // Buffer any data we want to send.
            messageWriter.WriteString(message);

            try
            {
                // Send the data as one complete message.
                await messageWriter.StoreAsync();
            }
            catch (Exception ex)
            {
                AppendOutputLine(MainPage.BuildWebSocketError(ex));
                AppendOutputLine(ex.Message);
                return;
            }

            //rootPage.NotifyUser("Send Complete", NotifyType.StatusMessage);
        }

        private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            // Dispatch the event to the UI thread so we can update UI.
            var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AppendOutputLine("Message Received; Type: " + args.MessageType);
                using (DataReader reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = UnicodeEncoding.Utf8;

                    try
                    {
                        string read = reader.ReadString(reader.UnconsumedBufferLength);
                        AppendOutputLine(read);
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(read);
                        HandleCommand(doc);
                    }
                    catch (Exception ex)
                    {
                        AppendOutputLine(MainPage.BuildWebSocketError(ex));
                        AppendOutputLine(ex.Message);
                    }
                }
            });
        }

        private void UpdateVisualState()
        {
            if (busy)
            {
                VisualStateManager.GoToState(this, "Busy", false);
            }
            else
            {
                bool connected = (messageWebSocket != null);
                VisualStateManager.GoToState(this, connected ? "Connected" : "Disconnected", false);
            }
        }


        // This may be triggered remotely by the server or locally by Close/Dispose()
        private async void OnClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            // Dispatch the event to the UI thread so we do not need to synchronize access to messageWebSocket.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AppendOutputLine("Closed; Code: " + args.Code + ", Reason: " + args.Reason);

                if (messageWebSocket == sender)
                {
                    CloseSocket();
                    UpdateVisualState();
                }
            });
        }

        private void SetBusy(bool value)
        {
            busy = value;
            UpdateVisualState();
        }

        private void OnDisconnect()
        {
            SetBusy(true);
            NotifyUser("Closing", NotifyType.StatusMessage);
            CloseSocket();
            SetBusy(false);
            NotifyUser("Closed", NotifyType.StatusMessage);
        }

        private async void OnConnect()
        {
            SetBusy(true);
            await ConnectAsync();
            SetBusy(false);
        }

        private Uri GetWsUri(Uri location)
        {
            string pathname = location.AbsolutePath;
            string pathname1 = pathname.Substring(0, pathname.LastIndexOf("/"));
            string pathname2 = pathname1.Substring(0, pathname1.LastIndexOf("/"));
            string search = location.Query;
            int pos = search.IndexOf("%3F");
            if (pos != -1)
            {
                search = search.Substring(0, pos);
            }
            return new Uri("ws://" + location.Authority + pathname2 + "/update" + search);
        }

        private async Task ConnectAsync()
        {
            Uri location = new Uri(Url);
            Uri server = GetWsUri(location);
            if (server == null)
            {
                return;
            }

            messageWebSocket = new MessageWebSocket();
            messageWebSocket.Control.MessageType = SocketMessageType.Utf8;
            messageWebSocket.MessageReceived += MessageReceived;
            messageWebSocket.Closed += OnClosed;

            AppendOutputLine($"Connecting to {server}...");
            try
            {
                await messageWebSocket.ConnectAsync(server);
            }
            catch (Exception ex) // For debugging
            {
                // Error happened during connect operation.
                messageWebSocket.Dispose();
                messageWebSocket = null;

                AppendOutputLine(MainPage.BuildWebSocketError(ex));
                AppendOutputLine(ex.Message);
                return;
            }

            // The default DataWriter encoding is Utf8.
            messageWriter = new DataWriter(messageWebSocket.OutputStream);
            NotifyUser("Connected", NotifyType.StatusMessage);
        }

        public static XmlElement getModelAttributes(XmlDocument doc, string ElementName, ResourceDictionary Model)
        {
            XmlElement element = doc.CreateElement(ElementName);
            foreach (KeyValuePair<object, object> kvp in Model)
            {
                if (kvp.Value is string)
                {
                    element.SetAttribute((string)kvp.Key, (string)kvp.Value);
                }
            }
            return element;
        }

        public static XmlElement ModelAsXml(XmlDocument doc, ResourceDictionary model)
        {
            XmlElement xmlmodel = getModelAttributes(doc, "model", model);

            foreach (KeyValuePair<object, object> kvp in model)
            {
                if (kvp.Value is ResourceDictionary)
                {
                    XmlElement entry = getModelAttributes(doc, "entry", (ResourceDictionary)kvp.Value);
                    entry.SetAttribute("target-id", (string)kvp.Key);
                    xmlmodel.AppendChild(entry);
                }
            }
            return xmlmodel;
        }


        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        private void UIElement_PointerEvent(string EventType, RoutedEventArgs e)
        {
            FrameworkElement f = e.OriginalSource as FrameworkElement;
            while (f != null && f.Name == "")
            {
                f = VisualTreeHelper.GetParent(f) as FrameworkElement;
            }
            if (f != null)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement xmlmodel = ModelAsXml(doc, model);
                xmlmodel.SetAttribute("dom-event-type", EventType);
                xmlmodel.SetAttribute("dom-event-click-id", f.Name);
                doc.AppendChild(xmlmodel);
                StringWriter strWriter = new StringWriter();
                doc.Save(strWriter);
                string xmlstr = strWriter.ToString();
                SendToWebsocket(xmlstr);
            }
        }

        private void UIElement_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            string EventType = null;
            switch (e.Pointer.PointerDeviceType)
            {
                case PointerDeviceType.Mouse:
                    EventType = "mousedown";
                    break;
                case PointerDeviceType.Touch:
                    EventType = "touchstart";
                    break;
            }
            if (EventType != null)
            {
                UIElement_PointerEvent(EventType, e);
            }
        }

        private void AddEvents(FrameworkElement game)
        {
            string Events = game.Resources["events"] as string;
            if (Events != null)
            {
                foreach (string Event in Events.Split(new char[] { ' ' }))
                {
                    Debug.WriteLine("Event: " + Event);
                    switch (Event)
                    {
                        case "PointerPressed":
                            game.PointerPressed += UIElement_PointerPressed;
                            break;
                    }
                }
            }
        }

        void DumpTree(DependencyObject obj, int indent = 0)
        {
            var spaces = new string(' ', indent);
            Debug.WriteLine($"{spaces}{obj.GetType().Name}");

            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                DumpTree(VisualTreeHelper.GetChild(obj, i), indent + 2);
            }
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            game = null;
            model = null;
            OnDisconnect();
            NotifyUser("", NotifyType.StatusMessage);
            // Create a request for the URL. 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string xaml = reader.ReadToEnd();
            FrameworkElement element = (FrameworkElement)XamlReader.Load(xaml);
            game = (FrameworkElement)element.FindName("game");
            if (game != null)
            {
                AddEvents(game);
                model = (ResourceDictionary)game.Resources["model"];
                if (model != null)
                {
                    OnConnect();
                }
            }
            Viewbox.Child = element;
            DumpTree(Viewbox);
        }

    }
}
