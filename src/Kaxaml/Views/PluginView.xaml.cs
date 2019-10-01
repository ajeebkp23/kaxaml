using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using Kaxaml.Core;
using Kaxaml.Plugins;
using Kaxaml.Plugins.Default;

namespace Kaxaml.Views
{
    /// <summary>
    /// Interaction logic for PuginView.xaml
    /// </summary>
    public partial class PluginView : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public const string PluginSubDir = "\\plugins";

        public PluginView()
        {
            InitializeComponent();
            LoadPlugins();
        }

        private void LoadPlugins()
        {
            // References
            Plugin references = new Plugin();
            references.Root = new References();
            references.Name = "References";
            references.Description = "Add references to Kaxaml";
            references.Key = Key.N;
            references.ModifierKeys = ModifierKeys.Control;
            references.Icon = LoadIcon(references.GetType(), "Images\\package_link.png");
            Plugins.Add(references);
            this.ReferencesPlugin = references;

            // load the snippets plugin
            Plugin snippets = new Plugin();
            snippets.Root = new Snippets();
            snippets.Name = "Snippets";
            snippets.Description = "Manage a set of commonly used snippets (Ctrl+N)";
            snippets.Key = Key.N;
            snippets.ModifierKeys = ModifierKeys.Control;
            snippets.Icon = LoadIcon(snippets.GetType(), "Images\\emb_tag.png");
            Plugins.Add(snippets);
            (App.Current as App).Snippets = snippets.Root as Snippets;

            //// add the find plugin 
            Plugin find = new Plugin();
            find.Root = new Find();
            find.Name = "Find";
            find.Description = "Find and replace text in the editor (Ctrl+F, F3)";
            find.Key = Key.F;
            find.ModifierKeys = ModifierKeys.Control;
            find.Icon = LoadIcon(snippets.GetType(), "Images\\find.png");
            Plugins.Add(find);
            _findPlugin = find;
            //FindInstance = (Find)find.Root;

            //// add the goto plugin
            //Plugin gotoline = new Plugin();
            //gotoline.Root = new Goto();
            //gotoline.Name = "Goto Line";
            //gotoline.Description = "Go to the specified line number in the editor.";
            //gotoline.Key = Key.G;
            //gotoline.ModifierKeys = ModifierKeys.Control;
            //Plugins.Add(gotoline);

            string PluginDir = App.StartupPath + PluginSubDir;

            // if the plugin directory doesn't exist, then we're done
            if (!Directory.Exists(PluginDir)) return;

            // get a pointer to the plugin directory
            DirectoryInfo d = new DirectoryInfo(PluginDir);

            // load each of the plugins in the directory
            foreach (FileInfo f in d.GetFiles("*.dll"))
            {
                var bytes = File.ReadAllBytes(f.FullName);
                Assembly asm = Assembly.Load(bytes);
                Type[] types = asm.GetExportedTypes();
                foreach (Type typ in types)
                {
                    var a = typ.GetCustomAttributes(typeof(PluginAttribute), false).Cast<PluginAttribute>().SingleOrDefault();
                    if (a != null && typeof(UserControl).IsAssignableFrom(typ))
                    {
                        Plugin p = new Plugin()
                        {
                            Root = (UserControl) Activator.CreateInstance(typ),
                            Name = a.Name,
                            Description = a.Description,
                            Key = a.Key,
                            ModifierKeys = a.ModifierKeys,
                            Icon = LoadIcon(typ, a.Icon)
                        };

                        Plugins.Add(p);
                    }
                }
            }

            //// add the settings plugin (we always want this to be at the end)
            Plugin settings = new Plugin();
            settings.Root = new Settings();
            settings.Name = "Settings";
            settings.Description = "Modify program settings and options (Ctrl+E)";
            settings.Key = Key.E;
            settings.ModifierKeys = ModifierKeys.Control;
            settings.Icon = LoadIcon(snippets.GetType(), "Images\\cog.png");
            Plugins.Add(settings);

            //// add the about plugin 
            Plugin about = new Plugin();
            about.Root = new About();
            about.Name = "About";
            about.Description = "All about Kaxaml";
            about.Icon = LoadIcon(snippets.GetType(), "Images\\kaxaml.png");
            Plugins.Add(about);

            //// add the settings plugin (we always want this to be at the end)
            //Plugin about = new Plugin();
            //about.Root = new About();
            //about.Name = "About";
            //about.Description = "All about Kaxaml.";
            //Plugins.Add(about);
        }

        private ImageSource LoadIcon(Type typ, string icon)
        {
            Assembly asm = Assembly.GetAssembly(typ);
            string iconString = typ.Namespace + '.' + icon.Replace('\\', '.');
            Stream myStream = asm.GetManifestResourceStream(iconString);

            if (myStream == null)
            {
                iconString = typ.Name + '.' + icon.Replace('\\', '.');
                myStream = asm.GetManifestResourceStream(iconString);
            }

            if (myStream == null)
            {
                iconString = "Kaxaml.Images.package.png";
                myStream = asm.GetManifestResourceStream(iconString);
            }

            if (myStream != null)
            {
                PngBitmapDecoder bitmapDecoder = new PngBitmapDecoder(myStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                if (bitmapDecoder.Frames[0] != null && bitmapDecoder.Frames[0] is ImageSource)
                {
                    return bitmapDecoder.Frames[0];
                }
                else
                {
                    return null;
                }
            }
            return null;
        }


        public List<Plugin> Plugins
        {
            get { return (List<Plugin>) GetValue(PluginsProperty); }
            set { SetValue(PluginsProperty, value); }
        }

        public static readonly DependencyProperty PluginsProperty =
            DependencyProperty.Register("Plugins", typeof(List<Plugin>), typeof(PluginView), new UIPropertyMetadata(new List<Plugin>()));

        public void OpenPlugin(Key key, ModifierKeys modifierkeys)
        {
            foreach (Plugin p in Plugins)
            {
                if (modifierkeys == p.ModifierKeys && key == p.Key)
                {
                    try
                    {
                        TabItem t = (TabItem) ((FrameworkElement) p.Root).Parent;
                        t.IsSelected = true;
                        t.Focus();

                        UpdateLayout();

                        if (t.Content is FrameworkElement)
                        {
                            (t.Content as FrameworkElement).MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.IsCriticalException())
                        {
                            throw;
                        }
                    }
                }
            }
        }

        Plugin _findPlugin = null;

        internal Plugin GetFindPlugin()
        {
            return _findPlugin;
        }

        private Plugin _referencesPlugin;

        public Plugin ReferencesPlugin
        {
            get { return _referencesPlugin; }
            set
            {
                if (Equals(value, _referencesPlugin)) return;
                _referencesPlugin = value;
                OnPropertyChanged();
            }
        }

        public Plugin SelectedPlugin
        {
            get { return (Plugin) PluginTabControl.SelectedItem; }
            set { PluginTabControl.SelectedItem = (Plugin) value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}