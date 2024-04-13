/* Title:           Main Menu
 * Date:            4-5-17
 * Author:          Terry Holmes
 * 
 * Description:     This is the main menu */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FindDuplicatePartNumbers
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        //setting up the class
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();

        public MainMenu()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //this will close the program
            TheMessagesClass.CloseTheProgram();
        }

        private void btnFindDuplicateParts_Click(object sender, RoutedEventArgs e)
        {
            FindDuplicates FindDuplicates = new FindDuplicates();
            FindDuplicates.Show();
            Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnFindDuplicateInventory_Click(object sender, RoutedEventArgs e)
        {
            FindDuplicateInventory FindDuplicateInventory = new FindDuplicateInventory();
            FindDuplicateInventory.Show();
            Close();
        }
    }
}
