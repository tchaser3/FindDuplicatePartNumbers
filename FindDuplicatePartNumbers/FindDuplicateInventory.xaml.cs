/* Title:           Find Duplicate Inventory Entries
 * Date:            4-12-17
 * Author:          Terry Holmes
 * 
 * Description:     This form is used to fix the incentory table */

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
using NewEventLogDLL;
using PartNumberDLL;
using InventoryDLL;
using DataValidationDLL;

namespace FindDuplicatePartNumbers
{
    /// <summary>
    /// Interaction logic for FindDuplicateInventory.xaml
    /// </summary>
    public partial class FindDuplicateInventory : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        EventLogClass TheEventLogClasss = new EventLogClass();
        PartNumberClass ThePartNumberClass = new PartNumberClass();
        InventoryClass TheInventoryClass = new InventoryClass();
        DataValidationClass TheDataValidationClass = new DataValidationClass();

        //setting up the data 
        WarehouseInventoryDataSet TheWarehouseInventoryDataSet;
        DuplicateInventoryDataSet TheDuplicateInventoryDataSet = new DuplicateInventoryDataSet();
        DuplicateInventoryDataSet TheSearchedDuplicateInventoryDataSet = new DuplicateInventoryDataSet();

        int gintPartCounter;
        int gintPartUpperLimit;

        public FindDuplicateInventory()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //setting local variables
            int intFirstCounter;
            int intSecondCounter;
            int intNumberOfRecords;
            int intTablePartID;
            int intWarehouseID;
            int intPartID;
            bool blnItemFound;
            int intPartCounter;
            bool blnItemAdded;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            try
            {
                //loading the ware inventory
                TheWarehouseInventoryDataSet = TheInventoryClass.GetWarehouseInventoryInfo();

                intNumberOfRecords = TheWarehouseInventoryDataSet.WarehouseInventory.Rows.Count - 1;
                gintPartCounter = 0;
                gintPartUpperLimit = 0;

                for (intFirstCounter = 0; intFirstCounter <= intNumberOfRecords; intFirstCounter++)
                {
                    if(TheWarehouseInventoryDataSet.WarehouseInventory[intFirstCounter].IsTablePartIDNull() == false)
                    {
                        blnItemFound = false;
                        blnItemAdded = false;
                        intTablePartID = TheWarehouseInventoryDataSet.WarehouseInventory[intFirstCounter].TablePartID;
                        intPartID = TheWarehouseInventoryDataSet.WarehouseInventory[intFirstCounter].PartID;
                        intWarehouseID = TheWarehouseInventoryDataSet.WarehouseInventory[intFirstCounter].WarehouseID;

                        if (gintPartCounter > 0)
                        {
                            for (intPartCounter = 0; intPartCounter <= gintPartUpperLimit; intPartCounter++)
                            {
                                if (intPartID == TheDuplicateInventoryDataSet.duplicateinventory[intPartCounter].PartID)
                                {
                                    blnItemFound = true;
                                }
                            }
                        }

                        if (blnItemFound == false)
                        {
                            for (intSecondCounter = 0; intSecondCounter <= intNumberOfRecords; intSecondCounter++)
                            {
                                if(TheWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].IsTablePartIDNull() == false)
                                {
                                    if (intTablePartID == TheWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].TablePartID)
                                    {
                                        if (intWarehouseID == TheWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].WarehouseID)
                                        {
                                            if (intPartID != TheWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].PartID)
                                            {
                                                DuplicateInventoryDataSet.duplicateinventoryRow NewTableRow = TheDuplicateInventoryDataSet.duplicateinventory.NewduplicateinventoryRow();

                                                NewTableRow.PartID = TheWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].PartID;
                                                NewTableRow.PartNumber = TheWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].PartNumber;
                                                NewTableRow.Quantity = TheWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].QTYOnHand;
                                                NewTableRow.TablePartID = TheWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].TablePartID;
                                                NewTableRow.WarehouseID = TheWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].WarehouseID;

                                                TheDuplicateInventoryDataSet.duplicateinventory.Rows.Add(NewTableRow);
                                                gintPartUpperLimit = gintPartCounter;
                                                gintPartCounter++;
                                                blnItemAdded = true;
                                            }
                                        }
                                    }
                                }
                            }

                            if (blnItemAdded == true)
                            {
                                DuplicateInventoryDataSet.duplicateinventoryRow NewTableRow = TheDuplicateInventoryDataSet.duplicateinventory.NewduplicateinventoryRow();

                                NewTableRow.PartID = TheWarehouseInventoryDataSet.WarehouseInventory[intFirstCounter].PartID;
                                NewTableRow.PartNumber = TheWarehouseInventoryDataSet.WarehouseInventory[intFirstCounter].PartNumber;
                                NewTableRow.Quantity = TheWarehouseInventoryDataSet.WarehouseInventory[intFirstCounter].QTYOnHand;
                                NewTableRow.TablePartID = TheWarehouseInventoryDataSet.WarehouseInventory[intFirstCounter].TablePartID;
                                NewTableRow.WarehouseID = TheWarehouseInventoryDataSet.WarehouseInventory[intFirstCounter].WarehouseID;

                                TheDuplicateInventoryDataSet.duplicateinventory.Rows.Add(NewTableRow);
                                gintPartUpperLimit = gintPartCounter;
                                gintPartCounter++;
                            }
                        }
                    }
                }

                dgrResults.ItemsSource = TheDuplicateInventoryDataSet.duplicateinventory;
            }
            catch (Exception Ex)
            {
                TheEventLogClasss.InsertEventLogEntry(DateTime.Now, "Find Duplicate Part Numbers \\ Find Duplicate Inventory \\ Window Loaded " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

            PleaseWait.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //this will close the program
            TheMessagesClass.CloseTheProgram();
        }

        private void btnMainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenu MainMenu = new MainMenu();
            MainMenu.Show();
            Close();
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            //setting local variables
            int intFirstCounter;
            int intPartCounter;
            int intPartID;
            int intTablePartID;
            int intWarehouseID;
            int intQuantity;
            int intNumberOfRecords;
            bool blnItemFound;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            try
            {
                intNumberOfRecords = TheDuplicateInventoryDataSet.duplicateinventory.Rows.Count - 1;
                gintPartCounter = 0;
                gintPartUpperLimit = 0;

                for(intFirstCounter = 0; intFirstCounter <= intNumberOfRecords; intFirstCounter++)
                {
                    intPartID = TheDuplicateInventoryDataSet.duplicateinventory[intFirstCounter].PartID;
                    intWarehouseID = TheDuplicateInventoryDataSet.duplicateinventory[intFirstCounter].WarehouseID;
                    intQuantity = TheDuplicateInventoryDataSet.duplicateinventory[intFirstCounter].Quantity;
                    intTablePartID = TheDuplicateInventoryDataSet.duplicateinventory[intFirstCounter].TablePartID;
                    blnItemFound = false;

                    if(gintPartCounter > 0)
                    {
                        for(intPartCounter = 0; intPartCounter <= gintPartUpperLimit; intPartCounter++)
                        {
                            if(intTablePartID == TheSearchedDuplicateInventoryDataSet.duplicateinventory[intPartCounter].TablePartID)
                            {
                                if(intWarehouseID == TheSearchedDuplicateInventoryDataSet.duplicateinventory[intPartCounter].WarehouseID)
                                {
                                    blnItemFound = true;
                                    TheSearchedDuplicateInventoryDataSet.duplicateinventory[intPartCounter].Quantity += intQuantity;
                                    TheInventoryClass.RemoveWarehouseInventoryRowByPartID(intPartID);
                                }
                            }
                        }
                    }

                    if(blnItemFound == false)
                    {
                        DuplicateInventoryDataSet.duplicateinventoryRow NewTableRow = TheSearchedDuplicateInventoryDataSet.duplicateinventory.NewduplicateinventoryRow();

                        NewTableRow.PartID = TheDuplicateInventoryDataSet.duplicateinventory[intFirstCounter].PartID;
                        NewTableRow.PartNumber = TheDuplicateInventoryDataSet.duplicateinventory[intFirstCounter].PartNumber;
                        NewTableRow.Quantity = TheDuplicateInventoryDataSet.duplicateinventory[intFirstCounter].Quantity;
                        NewTableRow.TablePartID = TheDuplicateInventoryDataSet.duplicateinventory[intFirstCounter].TablePartID;
                        NewTableRow.WarehouseID = TheDuplicateInventoryDataSet.duplicateinventory[intFirstCounter].WarehouseID;

                        TheSearchedDuplicateInventoryDataSet.duplicateinventory.Rows.Add(NewTableRow);
                        gintPartUpperLimit = gintPartCounter;
                        gintPartCounter++;
                    }
                }

                for(intPartCounter = 0; intPartCounter <= gintPartUpperLimit; intPartCounter++)
                {
                    intPartID = TheSearchedDuplicateInventoryDataSet.duplicateinventory[intPartCounter].PartID;
                    intQuantity = TheSearchedDuplicateInventoryDataSet.duplicateinventory[intPartCounter].Quantity;

                    TheInventoryClass.UpdateWarehouseInventoryRow(intPartID, intQuantity);
                }

                dgrResults.ItemsSource = TheSearchedDuplicateInventoryDataSet.duplicateinventory;
                btnProcess.IsEnabled = false;
            }
            catch (Exception Ex)
            {
                TheEventLogClasss.InsertEventLogEntry(DateTime.Now, "Find Duplicate Part Numbers \\ Find Duplicate Inventory \\ Process Button " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

            PleaseWait.Close();
        }
    }
}
