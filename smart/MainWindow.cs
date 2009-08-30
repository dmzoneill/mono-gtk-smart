using System;
using System.IO;
using System.Text.RegularExpressions;
using Gtk;
using Smart;
using System.Timers;

public partial class MainWindow: Gtk.Window
{	
	private StatusIcon myStatusIcon;
	private AboutDialog aboutDialog;
	private string forum_user;
	private string forum_password;
	private string modem_user;
	private string modem_password;
	private Timer updateTimer;
	private int interval = 3600;
	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{		
		Build ();
		
		this.myStatusIcon = new StatusIcon(new Gdk.Pixbuf("s.png"));
		this.myStatusIcon.Visible = true; 
		this.myStatusIcon.Tooltip = "Smart Broadband Usage";
		this.myStatusIcon.PopupMenu += OnStatusIconPopupMenu;
		this.myStatusIcon.Activate += delegate { this.Visible = !this.Visible; };
		this.image3.Pixbuf = new Gdk.Pixbuf("smart.png");
		this.WindowStateEvent += HandleWindowStateEvent;
		
		this.Iconify();
		
		label30.Text = "";
		
		if(this.readSettings()==true)
		{
			this.smartInitial();  // fire it off once at the start, then the timer will take over
			
			this.updateTimer = new Timer();
			this.updateTimer.Elapsed += new ElapsedEventHandler( smartRunner );
			this.updateTimer.Interval = this.interval;
			this.updateTimer.Start();	
		}
		else
		{
							
		}
	}
	
	

	protected void HandleWindowStateEvent(object o, WindowStateEventArgs args)
	{		
		if(args.Event.NewWindowState == Gdk.WindowState.Iconified)
		{
			this.Visible = false;						
			this.Deiconify();
		}
	}
	
	
	public void smartRunner(object source, ElapsedEventArgs e)
	{
		SmartTalker smart = new SmartTalker(this.forum_user, this.forum_password, this.modem_user, this.modem_password);
		
		if(smart.vbLogin() == true)
		{
			if(smart.bbUsageLogin() == true)
			{
				smart.parseBBUsage();
				smart.grabUsageImages();
				
				Application.Invoke( 
					delegate 
					{
						this.label21.Text = smart.summary[5];
						this.label19.Text = smart.summary[6];
						this.label20.Text = smart.summary[7];
						
						this.label25.Text = smart.summary[9];
						this.label24.Text = smart.summary[10];
						this.label23.Text = smart.summary[11];
						
						this.label29.Text = smart.summary[13];
						this.label28.Text = smart.summary[14];
						this.label27.Text = smart.summary[15];		
					}
				);
				
			}
			else
			{
				Console.WriteLine("Unable to login to retreive the broad band usage details");
			}			
		}
		else
		{
			Console.WriteLine("Unable to login to the forum");
		}		
		
		if(File.Exists("sum.png"))
		{
			Application.Invoke(
				delegate 
				{
					this.image2.Pixbuf = new Gdk.Pixbuf("sum.png");
				}
			);
		}
		
		if(File.Exists("daily.png"))
		{
			Application.Invoke(
				delegate 
				{
					this.image1.Pixbuf = new Gdk.Pixbuf("daily.png");
				}
			);
		}		
	}
	
	
	public void smartInitial()
	{
		SmartTalker smart = new SmartTalker(this.forum_user, this.forum_password, this.modem_user, this.modem_password);
		
		if(smart.vbLogin() == true)
		{
			if(smart.bbUsageLogin() == true)
			{
				smart.parseBBUsage();
				smart.grabUsageImages();
				
				Application.Invoke( 
					delegate 
					{
						this.label21.Text = smart.summary[5];
						this.label19.Text = smart.summary[6];
						this.label20.Text = smart.summary[7];
						
						this.label25.Text = smart.summary[9];
						this.label24.Text = smart.summary[10];
						this.label23.Text = smart.summary[11];
						
						this.label29.Text = smart.summary[13];
						this.label28.Text = smart.summary[14];
						this.label27.Text = smart.summary[15];		
					}
				);
				
			}
			else
			{
				Console.WriteLine("Unable to login to retreive the broad band usage details");
			}			
		}
		else
		{
			Console.WriteLine("Unable to login to the forum");
		}		
		
		if(File.Exists("sum.png"))
		{
			Application.Invoke(
				delegate 
				{
					this.image2.Pixbuf = new Gdk.Pixbuf("sum.png");
				}
			);
		}
		
		if(File.Exists("daily.png"))
		{
			Application.Invoke(
				delegate 
				{
					this.image1.Pixbuf = new Gdk.Pixbuf("daily.png");
				}
			);
		}		
	}
	
	
	private bool readSettings()
	{
		try
		{
			StreamReader Reader = new StreamReader("smart.txt");  
			string smartSettings = Reader.ReadToEnd();  
			Reader.Close(); 
			
			string[] lines = Regex.Split(smartSettings, "\n");
			
			this.entry1.Text = lines[0];
			this.entry2.Text = lines[1];
			this.entry3.Text = lines[2];
			this.entry4.Text = lines[3];
			this.forum_user = lines[0];
			this.forum_password = lines[1];
			this.modem_user = lines[2];
			this.modem_password = lines[3];
			this.combobox1.Active = int.Parse(lines[4]);
			this.combobox4.Active = int.Parse(lines[5]);
			this.combobox3.Active = int.Parse(lines[6]);
			
			if(int.Parse(lines[4])==0)
				this.interval = 3 * 60 * 60;
			else if(int.Parse(lines[4])==1)
				this.interval = 6 * 60 * 60;
			else if(int.Parse(lines[4])==2)
				this.interval = 12 * 60 * 60;
			else if(int.Parse(lines[4])==3)
				this.interval = 24 * 60 * 60;
			else 
				this.interval = 7 * 24 * 60 * 60;
			
			return true;
			
		}
		catch
		{
			return false;	
		}
	}
	
	
	protected void OnStatusIconPopupMenu(object sender, EventArgs e)
	{
		Menu popupMenu = new Menu();
		
		ImageMenuItem aboutitem = new ImageMenuItem ("About Smart Usage Viewer");
		Gtk.Image appimg1 = new Gtk.Image("s.png");
		aboutitem.Image = appimg1;
		aboutitem.Activated += new EventHandler(OnAboutActivated);
		popupMenu.Add(aboutitem);
		
		ImageMenuItem menuItemQuit = new ImageMenuItem ("Quit");		
		Gtk.Image appimg2 = new Gtk.Image(Stock.Quit, IconSize.Menu);
		menuItemQuit.Image = appimg2;
		menuItemQuit.Activated += new EventHandler(exitSmart);
		popupMenu.Add(menuItemQuit);
		
	   	popupMenu.ShowAll();
		popupMenu.Popup();

	}
	
	
	protected void OnAboutActivated(object sender, EventArgs e)
	{
		aboutDialog = new AboutDialog();
		aboutDialog.ProgramName = "Smart Broad Band Usage Viewer";
		aboutDialog.Version = "1.0 beta";
		aboutDialog.Comments = "Desktop application to moitor smart broadband usage";
		aboutDialog.License = "Creative Commons";
		aboutDialog.Authors = new string[] { "David O Neill dave@feeditout.com" };
		aboutDialog.Website = "http://www.feeditout.com";
		aboutDialog.Response += new ResponseHandler(OnHelloAboutClose);
		aboutDialog.Run();
	}


	protected void OnHelloAboutClose(object sender, ResponseArgs e)
	{
		if (e.ResponseId==ResponseType.Cancel || e.ResponseId==ResponseType.DeleteEvent)
		{
		 	aboutDialog.Destroy();
		}
	}


	protected virtual void OnButton1Clicked (object sender, System.EventArgs e)
	{
		StreamWriter tw = new StreamWriter("smart.txt");  
        tw.WriteLine(this.entry1.Text);
		tw.WriteLine(this.entry2.Text);
		tw.WriteLine(this.entry3.Text);
		tw.WriteLine(this.entry4.Text);
		tw.WriteLine(this.combobox1.Active);
		tw.WriteLine(this.combobox4.Active);
		tw.WriteLine(this.combobox3.Active);
        tw.Close();

		
		this.forum_user = this.entry1.Text;
		this.forum_password = this.entry2.Text;
		this.modem_user = this.entry3.Text;
		this.modem_password = this.entry4.Text;
			
		if(this.combobox1.Active==0)
			this.interval = 3 * 60 * 60;
		else if(this.combobox1.Active==1)
			this.interval = 6 * 60 * 60;
		else if(this.combobox1.Active==2)
			this.interval = 12 * 60 * 60;
		else if(this.combobox1.Active==3)
			this.interval = 24 * 60 * 60;
		else 
			this.interval = 7 * 24 * 60 * 60;
		
		this.smartInitial();  // fire it off once at the start, then the timer will take over
		
		try
		{
			this.updateTimer.Stop();
		}
		catch{}
			
		this.updateTimer = new Timer();
		this.updateTimer.Elapsed += new ElapsedEventHandler( smartRunner );
		this.updateTimer.Interval = this.interval;
		this.updateTimer.Start();	
		
	}

	protected virtual void OnExitActionActivated (object sender, System.EventArgs e)
	{
		exitSmart(this,new EventArgs());
	}
	
	public void exitSmart(object sender, EventArgs e)
	{
		this.updateTimer.Stop();
		Application.Quit();	
	}

	protected virtual void OnDeleteEvent (object o, Gtk.DeleteEventArgs args)
	{
		this.Visible = false;
		args.RetVal = true;
	}

	protected virtual void OnDestroyEvent (object o, Gtk.DestroyEventArgs args)
	{
		Console.WriteLine("destroy");
		args.RetVal = false;
	}
	
}