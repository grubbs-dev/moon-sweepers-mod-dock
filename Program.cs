using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace ModDock;

public static class Program
{
    [STAThread] public static void Main() => new Application().Run(new DockWindow());
}

public sealed class DockWindow : Window
{
    private readonly string settingsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MoonSweepersModDock", "settings.json");
    private readonly TextBox path = new();
    private readonly StackPanel mods = new();
    private readonly TextBlock status = new();
    private readonly TextBlock count = new();
    private readonly TextBlock health = new();
    private static readonly Brush Ink = new SolidColorBrush(Color.FromRgb(224, 235, 255));
    private static readonly Brush Muted = new SolidColorBrush(Color.FromRgb(145, 166, 195));
    private static readonly Brush Cyan = new SolidColorBrush(Color.FromRgb(105, 226, 244));
    private static readonly Brush Panel = new SolidColorBrush(Color.FromRgb(19, 30, 52));

    public DockWindow()
    {
        Title = "Moon Sweepers · Mod Dock"; Width = 1040; Height = 740; MinWidth = 800; MinHeight = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = new SolidColorBrush(Color.FromRgb(7, 13, 27)); Foreground = Ink;
        FontFamily = new FontFamily("Segoe UI"); FontSize = 14;
        var root = new Grid(); Content = root;
        var stars = new Canvas { IsHitTestVisible = false, ClipToBounds = true }; root.Children.Add(stars);
        var rng = new Random(17);
        for (var i=0;i<110;i++) { var dot=new Ellipse {Width=i%7==0?3:1.5,Height=i%7==0?3:1.5,Fill=Muted,Opacity=.15+rng.NextDouble()*.45}; Canvas.SetLeft(dot,rng.Next(1200));Canvas.SetTop(dot,rng.Next(850));stars.Children.Add(dot); }
        var layout = new Grid { Margin = new Thickness(36) }; root.Children.Add(layout);
        foreach(var h in new[]{"Auto","Auto","Auto","*","Auto"}) layout.RowDefinitions.Add(new RowDefinition {Height=h=="*"?new GridLength(1,GridUnitType.Star):GridLength.Auto});
        var header = new StackPanel(); header.Children.Add(Text("◒  MOON SWEEPERS",13,Cyan)); header.Children.Add(Text("MOD DOCK",36,Ink)); header.Children.Add(Text("Your launchpad for a modded Moon.",14,Muted)); Put(layout,header,0);
        var install = new StackPanel {Margin=new Thickness(0,24,0,18)};
        install.Children.Add(Text("01  /  GAME DIRECTORY",11,Cyan));
        var bar = new DockPanel {Margin=new Thickness(0,8,0,0)};
        var browse = Action("Browse…",Browse); DockPanel.SetDock(browse,Dock.Right);bar.Children.Add(browse);
        path.Background=Panel;path.Foreground=Ink;path.BorderBrush=Muted;path.Padding=new Thickness(12);path.VerticalContentAlignment=VerticalAlignment.Center;path.Margin=new Thickness(0,0,12,0);
        path.LostKeyboardFocus += (_,_)=>Refresh();bar.Children.Add(path);install.Children.Add(bar);
        health.Margin=new Thickness(0,8,0,0);health.Foreground=Muted;health.TextWrapping=TextWrapping.Wrap;install.Children.Add(health);Put(layout,install,1);
        var tools=new DockPanel {Margin=new Thickness(0,0,0,12)};
        var actions=new StackPanel{Orientation=System.Windows.Controls.Orientation.Horizontal};DockPanel.SetDock(actions,Dock.Right);
        actions.Children.Add(Action("+ Add DLL",Import));actions.Children.Add(Action("Refresh",Refresh));tools.Children.Add(actions);
        var label=new StackPanel();label.Children.Add(Text("02  /  INSTALLED MODS",11,Cyan));count.Foreground=Muted;label.Children.Add(count);tools.Children.Add(label);Put(layout,tools,2);
        var scroll=new ScrollViewer {VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Content=mods};Put(layout,scroll,3);
        var footer=new StackPanel {Margin=new Thickness(0,18,0,0)};
        var buttons=new WrapPanel();buttons.Children.Add(Action("↗ Launch Moon Sweepers",Launch,true));buttons.Children.Add(Action("Open plugins",()=>OpenFolder("BepInEx/plugins")));buttons.Children.Add(Action("View log",()=>OpenFile("BepInEx/LogOutput.log")));buttons.Children.Add(Action("Get BepInEx",()=>OpenUrl("https://builds.bepinex.dev/projects/bepinex_be/791")));buttons.Children.Add(Action("Get patcher",()=>OpenUrl("https://www.nexusmods.com/moonsweepers/mods/9")));footer.Children.Add(buttons);
        status.Foreground=Muted;status.TextWrapping=TextWrapping.Wrap;status.Margin=new Thickness(0,12,0,0);footer.Children.Add(status);Put(layout,footer,4);
        path.Text=LoadPath();Refresh();
    }
    private static TextBlock Text(string value,double size,Brush color)=>new(){Text=value,FontSize=size,Foreground=color,FontWeight=size>20?FontWeights.Bold:FontWeights.Normal};
    private static void Put(Grid grid,UIElement element,int row){Grid.SetRow(element,row);grid.Children.Add(element);}
    private Button Action(string title,Action action,bool primary=false)
    {
        var b=new Button {Content=title,Padding=new Thickness(14,10,14,10),Margin=new Thickness(0,0,8,0),Background=primary?Cyan:Panel,Foreground=primary?Background:Ink,BorderThickness=new Thickness(0),Cursor=System.Windows.Input.Cursors.Hand};
        b.Click+=(_,_)=>{try{action();}catch(Exception e){status.Text=e.Message;MessageBox.Show(this,e.Message,"Mod Dock",MessageBoxButton.OK,MessageBoxImage.Warning);}};return b;
    }
    private string GameDir=>System.IO.Path.GetFullPath(path.Text.Trim());
    private bool Valid()=>File.Exists(System.IO.Path.Combine(GameDir,"MoonSweepers.exe"));
    private void Require(){if(!Valid())throw new InvalidOperationException("Choose the folder containing MoonSweepers.exe first.");}
    private void RequireStopped()
    {
        Require();
        foreach(var p in Process.GetProcessesByName("MoonSweepers")){p.Dispose();throw new InvalidOperationException("Close Moon Sweepers before changing mods.");}
    }
    private string LoadPath()
    {
        try{var saved=JsonSerializer.Deserialize<string>(File.ReadAllText(settingsPath));if(saved!=null)return saved;}catch{ }
        foreach(var p in new[]{@"D:\Moon Sweepers Modding\Modded-1.0.5",@"D:\SteamLibrary\steamapps\common\Moon Sweepers"})if(File.Exists(System.IO.Path.Combine(p,"MoonSweepers.exe")))return p;
        return "";
    }
    private void Browse(){using var dialog=new System.Windows.Forms.FolderBrowserDialog {Description="Choose the folder containing MoonSweepers.exe"};if(dialog.ShowDialog()==System.Windows.Forms.DialogResult.OK){path.Text=dialog.SelectedPath;Refresh();}}
    private void Refresh()
    {
        mods.Children.Clear();
        if(string.IsNullOrWhiteSpace(path.Text)||!Valid()){health.Text="Select your Moon Sweepers folder to begin.";count.Text="No game selected";return;}
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(settingsPath)!);File.WriteAllText(settingsPath,JsonSerializer.Serialize(GameDir));
        var loader=File.Exists(System.IO.Path.Combine(GameDir,"BepInEx/core/BepInEx.Unity.IL2CPP.dll"))&&File.Exists(System.IO.Path.Combine(GameDir,"winhttp.dll"));
        var patcher=File.Exists(System.IO.Path.Combine(GameDir,"BepInEx/patchers/MoonSweepers.ModEngine/MoonSweepers.ModEngine.dll"));
        health.Text=$"BepInEx: {(loader?"detected":"missing")}   •   MoonSweepers patcher: {(patcher?"detected":"missing")}   •   Windows x64";
        var plugins=System.IO.Path.Combine(GameDir,"BepInEx/plugins");
        var files=Directory.Exists(plugins)?Directory.GetFiles(plugins,"*",SearchOption.AllDirectories).Where(f=>f.EndsWith(".dll",StringComparison.OrdinalIgnoreCase)||f.EndsWith(".dll.disabled",StringComparison.OrdinalIgnoreCase)).OrderBy(f=>f).ToArray():[];
        count.Text=$"{files.Count(f=>!f.EndsWith(".disabled"))} active / {files.Length} DLL files";
        if(files.Length==0)mods.Children.Add(Text("No mods in orbit yet. Add a mod DLL to get started.",16,Muted));
        foreach(var file in files)
        {
            bool enabled=!file.EndsWith(".disabled",StringComparison.OrdinalIgnoreCase);
            var line=new DockPanel {LastChildFill=true};
            var toggle=Action(enabled?"Disable":"Enable",()=>{RequireStopped();var target=enabled?file+".disabled":file[..^9];if(File.Exists(target))throw new IOException("The destination file already exists. Resolve the duplicate in the plugins folder.");File.Move(file,target);Refresh();status.Text="Mod state saved. Changes apply next launch.";});DockPanel.SetDock(toggle,Dock.Right);line.Children.Add(toggle);
            var detail=new StackPanel();detail.Children.Add(Text((enabled?"●  ":"○  ")+System.IO.Path.GetFileName(file).Replace(".dll.disabled","" ).Replace(".dll",""),18,enabled?Ink:Muted));detail.Children.Add(Text(System.IO.Path.GetRelativePath(plugins,file),12,Muted));line.Children.Add(detail);
            mods.Children.Add(new Border{Background=Panel,CornerRadius=new CornerRadius(10),Padding=new Thickness(18),Margin=new Thickness(0,0,0,10),Child=line});
        }
        status.Text="DLL manager for BepInEx • Enable/disable renames individual DLLs. Shared libraries may appear too.";
    }
    private void Import()
    {
        RequireStopped();var dialog=new Microsoft.Win32.OpenFileDialog {Filter="Mod DLL (*.dll)|*.dll",Multiselect=true,Title="Select mod DLLs (for ZIP mods, extract using their instructions)"};if(dialog.ShowDialog(this)!=true)return;
        foreach(var file in dialog.FileNames)
        {
            var name=System.IO.Path.GetFileNameWithoutExtension(file);var folder=System.IO.Path.Combine(GameDir,"BepInEx/plugins",name);var target=System.IO.Path.Combine(folder,System.IO.Path.GetFileName(file));
            if(Directory.Exists(System.IO.Path.Combine(GameDir,"BepInEx/plugins"))&&Directory.GetFiles(System.IO.Path.Combine(GameDir,"BepInEx/plugins"),System.IO.Path.GetFileName(file)+"*",SearchOption.AllDirectories).Length>0)throw new IOException($"{name} already exists. Update it manually with the game closed.");
            Directory.CreateDirectory(folder);File.Copy(file,target,false);
        }
        Refresh();status.Text="DLLs installed. Mods with assets or dependencies still need their full installation instructions.";
    }
    private void Launch()
    {
        Require();if(Process.GetProcessesByName("MoonSweepers").Length>0)throw new InvalidOperationException("Moon Sweepers is already running.");
        if(!File.Exists(System.IO.Path.Combine(GameDir,"BepInEx/core/BepInEx.Unity.IL2CPP.dll"))||!File.Exists(System.IO.Path.Combine(GameDir,"BepInEx/patchers/MoonSweepers.ModEngine/MoonSweepers.ModEngine.dll")))throw new InvalidOperationException("Install BepInEx and the required MoonSweepers patcher first. Use the download links below.");
        var appid=System.IO.Path.Combine(GameDir,"steam_appid.txt");if(!File.Exists(appid))File.WriteAllText(appid,"5335510");
        Process.Start(new ProcessStartInfo(System.IO.Path.Combine(GameDir,"MoonSweepers.exe")){WorkingDirectory=GameDir,UseShellExecute=true});status.Text="Launch requested. Keep Steam running; the game may take a moment to load.";
    }
    private void OpenFolder(string relative){Require();var folder=System.IO.Path.Combine(GameDir,relative);Directory.CreateDirectory(folder);Process.Start(new ProcessStartInfo(folder){UseShellExecute=true});}
    private void OpenFile(string relative){Require();var file=System.IO.Path.Combine(GameDir,relative);if(!File.Exists(file))throw new FileNotFoundException("No log yet. Launch the game once.");Process.Start(new ProcessStartInfo(file){UseShellExecute=true});}
    private static void OpenUrl(string url)=>Process.Start(new ProcessStartInfo(url){UseShellExecute=true});
}
