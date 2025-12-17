using System.Windows;
using CarRental.Data.Models;

namespace CarRental.BackOffice
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
        }
    }
    
    
    
}