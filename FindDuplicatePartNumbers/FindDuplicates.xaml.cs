/* Title:           Find Duplicates
 * Date:            4-4-17
 * Author:          Terry Holmes
 * 
 * Description:     This will find duplicate part numbers */

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
using PartNumberDLL;
using InventoryDLL;
using ReceivedMaterialDLL;
using IssuedPartsDLL;
using BOMPartsDLL;
using NewEventLogDLL;
using KeyWordDLL;

namespace FindDuplicatePartNumbers
{
    /// <summary>
    /// Interaction logic for FindDuplicates.xaml
    /// </summary>
    public partial class FindDuplicates : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        PartNumberClass ThePartNumberClass = new PartNumberClass();
        InventoryClass TheInventoryClass = new InventoryClass();
        ReceivedMaterialClass TheReceivedMaterialClass = new ReceivedMaterialClass();
        IssuedPartsClass TheIssuedPartsClass = new IssuedPartsClass();
        BOMPartsClass TheBOMPartsClass = new BOMPartsClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        KeyWordClass TheKeyWordClass = new KeyWordClass();

        //setting up the data
        PartNumbersDataSet ThePartNumbersDataSet;
        DuplicatePartDataSet TheDuplicatePartDataSet = new DuplicatePartDataSet();
        DuplicatePartDataSet TheSearchedDuplicatePartDataSet = new DuplicatePartDataSet();
        WarehouseInventoryDataSet TheKeptWarehouseInventoryDataSet = new WarehouseInventoryDataSet();
        WarehouseInventoryDataSet TheRemovedWarehouseInventoryDataSet = new WarehouseInventoryDataSet();
        IssuedPartsDataSet TheIssuedPartsDataSet;
        ReceivedPartsDataSet TheReceivedPartsDataSet;
        BOMPartsDataSet TheBOMPartsDataSet;
        PartKeptDataSet ThePartKeptDataSet = new PartKeptDataSet();
        PartKeptDataSet ThePartRemovedDataSet = new PartKeptDataSet();

        //setting global variables
        int gintPartUpperLimit;
        string gstrErrorMessage;
        int gintDuplicateCounter;
        int gintDuplicateUpperLimit;
        int[] gintBadPartID;

        public FindDuplicates()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //this will close the program
            TheMessagesClass.CloseTheProgram();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this will load during the form load
            //setting local variables
            bool blnFatalError = false;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            cboSelectPart.Items.Add("Select Part Number");

            blnFatalError = LoadPartDataSet();
            if (blnFatalError == false)
                blnFatalError = FindDuplicateParts();

            PleaseWait.Close();

            dgrDuplicates.ItemsSource = TheDuplicatePartDataSet.duplicates;
            cboSelectPart.SelectedIndex = 0;
            btnProcess.IsEnabled = false;

            if(blnFatalError == true)
            {
                TheMessagesClass.ErrorMessage(gstrErrorMessage);
                btnProcess.IsEnabled = false;
                btnFindJDEDuplicates.IsEnabled = false;
            }
        }
        public bool FindDuplicateParts()
        {
            bool blnFatalError = false;
            int intPartCounter;
            int intSecondPartCounter;
            bool blnDuplicateFound;
            string strPartNumber;
            string strDescription;
            string strJDEPartNumber;
            int intPartID;
            bool blnKeyWordNotFound;

            try
            {
                //setting up for the loop
                gintDuplicateCounter = 0;
                gintDuplicateUpperLimit = 0;
                TheDuplicatePartDataSet.duplicates.Rows.Clear();
                cboSelectPart.Items.Clear();

                cboSelectPart.Items.Add("Select Part Number");

                for (intPartCounter = 0; intPartCounter <= gintPartUpperLimit; intPartCounter++)
                {
                    intPartID = ThePartNumbersDataSet.partnumbers[intPartCounter].PartID;
                    strPartNumber = ThePartNumbersDataSet.partnumbers[intPartCounter].PartNumber;
                    strDescription = ThePartNumbersDataSet.partnumbers[intPartCounter].Description;
                    strJDEPartNumber = ThePartNumbersDataSet.partnumbers[intPartCounter].JDEPartNumber;
                    blnDuplicateFound = false;
                    
                    for(intSecondPartCounter = 0; intSecondPartCounter <= gintPartUpperLimit; intSecondPartCounter++)
                    {
                        blnKeyWordNotFound = TheKeyWordClass.FindKeyWord(strPartNumber, ThePartNumbersDataSet.partnumbers[intSecondPartCounter].PartNumber);

                        if(blnKeyWordNotFound == false)
                        {
                            if(intPartID != ThePartNumbersDataSet.partnumbers[intSecondPartCounter].PartID)
                            {
                                blnDuplicateFound = true;
                                EnterDuplicateRecord(ThePartNumbersDataSet.partnumbers[intSecondPartCounter].PartID, ThePartNumbersDataSet.partnumbers[intSecondPartCounter].PartNumber, ThePartNumbersDataSet.partnumbers[intSecondPartCounter].JDEPartNumber, ThePartNumbersDataSet.partnumbers[intSecondPartCounter].Description);
                            }
                        }
                    }

                    if(blnDuplicateFound == true)
                    {
                        cboSelectPart.Items.Add(strPartNumber);
                        EnterDuplicateRecord(intPartID, strPartNumber, strJDEPartNumber, strDescription);
                    }
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Find Duplicate Part Number \\ Find Duplicates \\ Find Duplicate Parts " + Ex.Message);

                gstrErrorMessage = Ex.ToString();

                blnFatalError = true;
            }

            return blnFatalError;
        }
        private void EnterDuplicateRecord(int intPartID, string strPartNumber, string strJDEPartNumber, string strDescription)
        {
            int intCounter;
            bool blnItemNotFound = true;

            if(gintDuplicateCounter > 0)
            {
                for(intCounter = 0; intCounter <= gintDuplicateUpperLimit; intCounter++)
                {
                    if(intPartID == TheDuplicatePartDataSet.duplicates[intCounter].PartID)
                    {
                        blnItemNotFound = false;
                    }
                }
            }

            if(blnItemNotFound == true)
            {
                DuplicatePartDataSet.duplicatesRow NewDuplicateRow = TheDuplicatePartDataSet.duplicates.NewduplicatesRow();

                NewDuplicateRow.Description = strDescription;
                NewDuplicateRow.PartID = intPartID;
                NewDuplicateRow.Keep = false;
                NewDuplicateRow.JDEPartNumber = strJDEPartNumber;
                NewDuplicateRow.PartNumber = strPartNumber;
                NewDuplicateRow.Remove = false;

                TheDuplicatePartDataSet.duplicates.Rows.Add(NewDuplicateRow);

                gintDuplicateUpperLimit = gintDuplicateCounter;
                gintDuplicateCounter++;
            }

            
        }
        private bool LoadPartDataSet()
        {
            bool blnFatalError = false;

            try
            {
                ThePartNumbersDataSet = ThePartNumberClass.GetPartNumbersInfo();

                gintPartUpperLimit = ThePartNumbersDataSet.partnumbers.Rows.Count - 1;

                gintBadPartID = new int[gintPartUpperLimit + 1];
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Find Duplicate Part Numbers Find Duplicates Load Part Data Set " + Ex.Message);

                gstrErrorMessage = Ex.ToString();

                blnFatalError = true;
            }

            return blnFatalError;
        }

        private void btnMainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenu MainMenu = new MainMenu();
            MainMenu.Show();
            Close();
        }
        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            //this will process  and remove the duplicate part numbers
            //setting local variables
            int intDupeCounter;
            int intDupeNumberOfRecords;
            int intRecordsReturned;
            int intNewPartID = 0;
            int intTablePartID;
            bool blnFatalError;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            try
            {
                intDupeNumberOfRecords = TheSearchedDuplicatePartDataSet.duplicates.Rows.Count - 1;

                //loop
                for(intDupeCounter = 0; intDupeCounter <= intDupeNumberOfRecords; intDupeCounter++)
                {
                    if(TheSearchedDuplicatePartDataSet.duplicates[intDupeCounter].Remove == false)
                    {
                        if(TheSearchedDuplicatePartDataSet.duplicates[intDupeCounter].Keep == true)
                        {
                            intNewPartID = TheSearchedDuplicatePartDataSet.duplicates[intDupeCounter].PartID;

                            TheKeptWarehouseInventoryDataSet = TheInventoryClass.GetWarehouseCompleteInventoryByPartID(intNewPartID);
                        }
                    }
                }

                blnFatalError = CheckForDuplicates();

                if(blnFatalError == true)
                {
                    throw new Exception("Problems with Duplicates");
                }

                //loop to find items to remove
                for (intDupeCounter = 0; intDupeCounter <= intDupeNumberOfRecords; intDupeCounter++)
                {
                    if(TheSearchedDuplicatePartDataSet.duplicates[intDupeCounter].Keep == false)
                    {
                        if(TheSearchedDuplicatePartDataSet.duplicates[intDupeCounter].Remove == true)
                        {
                            intTablePartID = TheSearchedDuplicatePartDataSet.duplicates[intDupeCounter].PartID;

                            TheRemovedWarehouseInventoryDataSet = TheInventoryClass.GetWarehouseCompleteInventoryByPartID(intTablePartID);

                            intRecordsReturned = TheRemovedWarehouseInventoryDataSet.WarehouseInventory.Rows.Count;

                            if(intRecordsReturned == 0)
                            {
                                blnFatalError = ThePartNumberClass.RemovePartFromPartList(intTablePartID);

                                if(blnFatalError == true)
                                {
                                    throw new Exception("Problem With Delete");
                                }
                            }
                            else
                            {
                                blnFatalError = CheckForTransactions(intNewPartID);

                                if(blnFatalError == true)
                                {
                                    throw new Exception("Problems with Transactions");
                                }
                                else
                                {
                                    blnFatalError = UpdateWarehouseCounts(intNewPartID);

                                    if (blnFatalError == true)
                                    {
                                        throw new Exception("Problems with Updating the Count");
                                    }
                                }
                            }
                        }
                    }
                }

                LoadPartDataSet();
                FindDuplicateParts();
                dgrDuplicates.ItemsSource = TheDuplicatePartDataSet.duplicates;
                TheMessagesClass.InformationMessage("Item Updated");
                cboSelectPart.SelectedIndex = 0;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Find Duplicate Part Numbers \\ Find Duplicates \\ Process Button " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

            PleaseWait.Close();
        }
        private bool UpdateWarehouseCounts(int intNewPartID)
        {
            bool blnFatalError = false;
            int intKeepCounter;
            int intKeepNumberOfRecords;
            int intRemoveCounter;
            int intRemoveNumberOfRecords;
            bool blnRecordNotFound;
            int intQuantity = 0;
            int intWarehouseID;
            int intTablePartID;
            int intPartID;

            try
            {
                intKeepNumberOfRecords = TheKeptWarehouseInventoryDataSet.WarehouseInventory.Rows.Count - 1;
                intRemoveNumberOfRecords = TheRemovedWarehouseInventoryDataSet.WarehouseInventory.Rows.Count - 1;

                //remove loop
                for(intRemoveCounter = 0; intRemoveCounter <= intRemoveNumberOfRecords; intRemoveCounter++)
                {
                    blnRecordNotFound = true;
                    intTablePartID = TheRemovedWarehouseInventoryDataSet.WarehouseInventory[intRemoveCounter].TablePartID;
                    intWarehouseID = TheRemovedWarehouseInventoryDataSet.WarehouseInventory[intRemoveCounter].WarehouseID;
                    intPartID = TheRemovedWarehouseInventoryDataSet.WarehouseInventory[intRemoveCounter].PartID;
                    intQuantity = TheRemovedWarehouseInventoryDataSet.WarehouseInventory[intRemoveCounter].QTYOnHand;

                    //keep loop
                    for(intKeepCounter = 0; intKeepCounter <= intKeepNumberOfRecords; intKeepCounter++)
                    {
                        if(intWarehouseID == TheKeptWarehouseInventoryDataSet.WarehouseInventory[intKeepCounter].WarehouseID)
                        {
                            TheKeptWarehouseInventoryDataSet.WarehouseInventory[intKeepCounter].QTYOnHand += intQuantity;
                            blnRecordNotFound = false;
                        }
                    }

                    if(blnRecordNotFound == false)
                    {
                        blnFatalError = TheInventoryClass.RemoveWarehouseInventoryRowByPartID(intPartID);

                        if(blnFatalError == true)
                        {
                            throw new Exception("Problems With Removing Warehouse Inventory Rows ");
                        }

                        blnFatalError = ThePartNumberClass.RemovePartFromPartList(intTablePartID);
                    }
                    if(blnRecordNotFound == true)
                    {
                        blnFatalError = TheInventoryClass.UpdateWarehouseInventoryTablePartID(intPartID, intNewPartID);

                        if (blnFatalError == true)
                        {
                            throw new Exception("Problems With Removing Warehouse Inventory Rows ");
                        }

                        blnFatalError = ThePartNumberClass.RemovePartFromPartList(intTablePartID);
                    }
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Find Duplicate Part Numbers \\ Find Duplicates \\ Update Warehouse Counts " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

            return blnFatalError;
        }
        private bool CheckForDuplicates()
        {
            bool blnFatalError = false;
            int intFirstCounter;
            int intSecondCounter;
            int intNumberOfRecords;
            int intTablePartID;
            int intWarehouseID;
            int intPartID;
            int intQuantity;

            try
            {
                //getting ready for the loop
                intNumberOfRecords = TheKeptWarehouseInventoryDataSet.WarehouseInventory.Rows.Count - 1;

                for(intFirstCounter = 0; intFirstCounter <= intNumberOfRecords; intFirstCounter++)
                {
                    intPartID = TheKeptWarehouseInventoryDataSet.WarehouseInventory[intFirstCounter].PartID;
                    intWarehouseID = TheKeptWarehouseInventoryDataSet.WarehouseInventory[intFirstCounter].WarehouseID;
                    intTablePartID = TheKeptWarehouseInventoryDataSet.WarehouseInventory[intFirstCounter].TablePartID;

                    //second counter
                    for(intSecondCounter = 0; intSecondCounter <= intNumberOfRecords; intSecondCounter++)
                    {
                        if(intPartID != TheKeptWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].PartID)
                        {
                            if(intWarehouseID == TheKeptWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].WarehouseID)
                            {
                                if(intTablePartID == TheKeptWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].TablePartID)
                                {
                                    intQuantity = TheKeptWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].QTYOnHand;

                                    TheKeptWarehouseInventoryDataSet.WarehouseInventory[intFirstCounter].QTYOnHand += intQuantity;

                                    blnFatalError = TheInventoryClass.RemoveWarehouseInventoryRowByPartID(TheKeptWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].PartID);

                                    TheKeptWarehouseInventoryDataSet.WarehouseInventory.Rows[intSecondCounter].Delete();
                                    TheKeptWarehouseInventoryDataSet.AcceptChanges();

                                    intSecondCounter--;
                                    intNumberOfRecords--;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Find Duplicate Part Numbers \\ Find Duplicates \\ Check For Duplicates " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

            return blnFatalError;
        }
        private bool CheckForTransactions(int intNewPartID)
        {
            int intWarehouseCounter;
            int intWarehouseUpperLimit;
            int intTransactionCounter;
            int intTransactionUpperLimit;
            int intTransactionID;
            int intPartID;
            int intWarehouseID;
            bool blnFatalError = false;

            try
            {
                //getting the count;
                intWarehouseUpperLimit = TheRemovedWarehouseInventoryDataSet.WarehouseInventory.Rows.Count - 1;

                for(intWarehouseCounter = 0; intWarehouseCounter <= intWarehouseUpperLimit; intWarehouseCounter++)
                {
                    intPartID = TheRemovedWarehouseInventoryDataSet.WarehouseInventory[intWarehouseCounter].TablePartID;
                    intWarehouseID = TheRemovedWarehouseInventoryDataSet.WarehouseInventory[intWarehouseCounter].WarehouseID;

                    //beginning with the received table
                    TheReceivedPartsDataSet = TheReceivedMaterialClass.GetReceivedPartsByPartID(intPartID, intWarehouseID);

                    intTransactionUpperLimit = TheReceivedPartsDataSet.ReceivedParts.Rows.Count - 1;

                    if(intTransactionUpperLimit > -1)
                    {
                        for(intTransactionCounter = 0; intTransactionCounter <= intTransactionUpperLimit; intTransactionCounter++)
                        {
                            intTransactionID = TheReceivedPartsDataSet.ReceivedParts[intTransactionCounter].TransactionID;

                            blnFatalError = TheReceivedMaterialClass.UpdateReceivedPartsPartID(intNewPartID, intTransactionID);

                            if(blnFatalError == true)
                            {
                                throw new Exception("There Was A Problem with Receive Transactions");
                            }
                        }
                    }

                    TheIssuedPartsDataSet = TheIssuedPartsClass.GetIssuedPartsByPartID(intPartID, intWarehouseID);

                    intTransactionUpperLimit = TheIssuedPartsDataSet.IssuedParts.Rows.Count - 1;

                    if (intTransactionUpperLimit > -1)
                    {
                        for (intTransactionCounter = 0; intTransactionCounter <= intTransactionUpperLimit; intTransactionCounter++)
                        {
                            intTransactionID = TheIssuedPartsDataSet.IssuedParts[intTransactionCounter].TransactionID;

                            blnFatalError = TheIssuedPartsClass.UpdateIssuedPartsPartID(intNewPartID, intTransactionID);

                            if (blnFatalError == true)
                            {
                                throw new Exception("There Was A Problem with Issued Transactions");
                            }
                        }
                    }

                    TheBOMPartsDataSet = TheBOMPartsClass.GetBOMPartsByPartID(intPartID, intWarehouseID);

                    intTransactionUpperLimit = TheBOMPartsDataSet.BOMParts.Rows.Count - 1;

                    if (intTransactionUpperLimit > -1)
                    {
                        for (intTransactionCounter = 0; intTransactionCounter <= intTransactionUpperLimit; intTransactionCounter++)
                        {
                            intTransactionID = TheBOMPartsDataSet.BOMParts[intTransactionCounter].TransactionID;

                            blnFatalError = TheBOMPartsClass.UpdateBOMPartID(intTransactionID, intPartID);

                            if (blnFatalError == true)
                            {
                                throw new Exception("There Was A Problem with BOM Transactions");
                            }
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Find Duplicate Part Numbers \\ Find Duplicates \\ Check For Transactions " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());

                blnFatalError = true;
            }

            return blnFatalError;
        }
        private void cboSelectPart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //setting local variables
            int intCounter;
            int intNumberOfRecords;
            string strPartNumber;
            bool blnKeyWordNotFound;

            try
            {
                if(cboSelectPart.SelectedIndex > -1)
                {
                    strPartNumber = cboSelectPart.SelectedItem.ToString();
                    TheSearchedDuplicatePartDataSet.duplicates.Rows.Clear();
                }
                else
                {
                    strPartNumber = "Select Part Number";
                }
                

                if(strPartNumber != "Select Part Number")
                {
                    intNumberOfRecords = TheDuplicatePartDataSet.duplicates.Rows.Count - 1;

                    for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        blnKeyWordNotFound = TheKeyWordClass.FindKeyWord(strPartNumber, TheDuplicatePartDataSet.duplicates[intCounter].PartNumber);

                        if(blnKeyWordNotFound == false)
                        {
                            DuplicatePartDataSet.duplicatesRow NewDuplicateRow = TheSearchedDuplicatePartDataSet.duplicates.NewduplicatesRow();

                            NewDuplicateRow.Description = TheDuplicatePartDataSet.duplicates[intCounter].Description;
                            NewDuplicateRow.JDEPartNumber = TheDuplicatePartDataSet.duplicates[intCounter].JDEPartNumber;
                            NewDuplicateRow.Keep = TheDuplicatePartDataSet.duplicates[intCounter].Keep;
                            NewDuplicateRow.PartID = TheDuplicatePartDataSet.duplicates[intCounter].PartID;
                            NewDuplicateRow.PartNumber = TheDuplicatePartDataSet.duplicates[intCounter].PartNumber;
                            NewDuplicateRow.Remove = TheDuplicatePartDataSet.duplicates[intCounter].Remove;

                            TheSearchedDuplicatePartDataSet.duplicates.Rows.Add(NewDuplicateRow);
                        }
                    }

                    dgrDuplicates.ItemsSource = TheSearchedDuplicatePartDataSet.duplicates;
                    btnProcess.IsEnabled = true;
                }

                
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now,"Find Duplicate Part Numbers \\ Find Duplicates \\ cbo Select Part Selection Changed " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
        }

        private void btnFindJDEDuplicates_Click(object sender, RoutedEventArgs e)
        {
            //this will find duplicate JDE Part Numbers
            FindDuplicateJDEPartNumbers FindDuplicateJDEPartNumbers = new FindDuplicateJDEPartNumbers();
            FindDuplicateJDEPartNumbers.ShowDialog();
        }
    }
}
