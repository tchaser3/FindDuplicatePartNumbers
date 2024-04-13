/* Title:           Find Duplicate JDE Part Numbers
 * Date:            4-12-17
 * Author:          Terry Holmes
 * 
 * Description:     This form is to find the duplicate JDE Part Numbers */

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
using InventoryDLL;
using PartNumberDLL;
using NewEventLogDLL;
using ReceivedMaterialDLL;
using IssuedPartsDLL;
using BOMPartsDLL;

namespace FindDuplicatePartNumbers
{
    /// <summary>
    /// Interaction logic for FindDuplicateJDEPartNumbers.xaml
    /// </summary>
    public partial class FindDuplicateJDEPartNumbers : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        InventoryClass TheInventoryClass = new InventoryClass();
        PartNumberClass ThePartNumberClass = new PartNumberClass();
        ReceivedMaterialClass TheReceivedMaterialClass = new ReceivedMaterialClass();
        IssuedPartsClass TheIssuedPartsClass = new IssuedPartsClass();
        BOMPartsClass TheBOMPartsClass = new BOMPartsClass();

        //setting up the data sets
        PartNumbersDataSet ThePartNumbersDataSet;
        DuplicatePartDataSet TheDuplicatePartNumberDataSet = new DuplicatePartDataSet();
        DuplicatePartDataSet TheSearchedDuplicatePartNumbers = new DuplicatePartDataSet();
        WarehouseInventoryDataSet TheKeptWarehouseInventoryDataSet = new WarehouseInventoryDataSet();
        WarehouseInventoryDataSet TheRemovedWarehouseInventoryDataSet = new WarehouseInventoryDataSet();
        ReceivedPartsDataSet TheReceivedPartsDataSet = new ReceivedPartsDataSet();
        IssuedPartsDataSet TheIssuedPartsDataSet = new IssuedPartsDataSet();
        BOMPartsDataSet TheBOMPartsDataSet = new BOMPartsDataSet();

        string[] gstrJDEPartNumber;
        int gintPartCounter;
        int gintPartUpperLimit;
        string gstrErrorMessage;

        public FindDuplicateJDEPartNumbers()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //setting local variables
            int intFirstCounter;
            int intSecondCounter;
            int intNumberOfRecords;
            int intPartID;
            string strJDEPartNumber;
            bool blnPartFound;
            int intPartCounter;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            try
            {
                //getting the part number info
                ThePartNumbersDataSet = ThePartNumberClass.GetPartNumbersInfo();
                btnProcess.IsEnabled = false;

                //getting the record count
                intNumberOfRecords = ThePartNumbersDataSet.partnumbers.Rows.Count - 1;
                gstrJDEPartNumber = new string[intNumberOfRecords];
                gintPartCounter = 0;
                gintPartUpperLimit = 0;
                cboSelectJDEPartNumber.Items.Add("Select JDE Part Number");

                //first loop
                for(intFirstCounter = 0; intFirstCounter <= intNumberOfRecords; intFirstCounter++)
                {
                    //getting search variables
                    intPartID = ThePartNumbersDataSet.partnumbers[intFirstCounter].PartID;
                    strJDEPartNumber = ThePartNumbersDataSet.partnumbers[intFirstCounter].JDEPartNumber;
                    blnPartFound = false;
                    
                    if(strJDEPartNumber != "NOT REQUIRED")
                    {
                        if(strJDEPartNumber != "NOT PROVIDED")
                        {
                            //second loop
                            for (intSecondCounter = 0; intSecondCounter <= intNumberOfRecords; intSecondCounter++)
                            {
                                if (strJDEPartNumber == ThePartNumbersDataSet.partnumbers[intSecondCounter].JDEPartNumber)
                                {
                                    if (intPartID != ThePartNumbersDataSet.partnumbers[intSecondCounter].PartID)
                                    {
                                        if(gintPartCounter > 0)
                                        {
                                            for(intPartCounter = 0; intPartCounter <= gintPartUpperLimit; intPartCounter++)
                                            {
                                                if(gstrJDEPartNumber[intPartCounter] == strJDEPartNumber)
                                                {
                                                    blnPartFound = true;
                                                }
                                            }
                                            if(blnPartFound == false)
                                            {
                                                cboSelectJDEPartNumber.Items.Add(strJDEPartNumber);
                                            }
                                        }

                                        DuplicatePartDataSet.duplicatesRow NewDuplicateRow = TheDuplicatePartNumberDataSet.duplicates.NewduplicatesRow();

                                        NewDuplicateRow.Description = ThePartNumbersDataSet.partnumbers[intSecondCounter].Description;
                                        NewDuplicateRow.JDEPartNumber = strJDEPartNumber;
                                        NewDuplicateRow.Keep = false;
                                        NewDuplicateRow.PartID = ThePartNumbersDataSet.partnumbers[intSecondCounter].PartID;
                                        NewDuplicateRow.PartNumber = ThePartNumbersDataSet.partnumbers[intSecondCounter].PartNumber;
                                        NewDuplicateRow.Remove = false;

                                        TheDuplicatePartNumberDataSet.duplicates.Rows.Add(NewDuplicateRow);
                                        gintPartUpperLimit = gintPartCounter;
                                        gintPartCounter++;
                                    }
                                }
                            }
                        }
                    }
                }

                dgrResults.ItemsSource = TheDuplicatePartNumberDataSet.duplicates;
                cboSelectJDEPartNumber.SelectedIndex = 0;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Find Duplicate Part Numbes \\ Find Duplicate JDE Part Numbes \\ Window Loaded " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

            PleaseWait.Close();
        }

        private void cboSelectJDEPartNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //setting local variables
            string strJDEPartNumber;
            int intCounter;
            int intNumberOfRecords;
            int intSelectedIndex;

            //getting ready for the loop
            intSelectedIndex = cboSelectJDEPartNumber.SelectedIndex;

            if(intSelectedIndex > -1)
            {
                strJDEPartNumber = cboSelectJDEPartNumber.SelectedItem.ToString();

                if(strJDEPartNumber != "Select JDE Part Number")
                {
                    intNumberOfRecords = TheDuplicatePartNumberDataSet.duplicates.Rows.Count - 1;
                    TheSearchedDuplicatePartNumbers.duplicates.Rows.Clear();

                    for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        if(strJDEPartNumber == TheDuplicatePartNumberDataSet.duplicates[intCounter].JDEPartNumber)
                        {
                            DuplicatePartDataSet.duplicatesRow NewDuplicateRow = TheSearchedDuplicatePartNumbers.duplicates.NewduplicatesRow();

                            NewDuplicateRow.Description = TheDuplicatePartNumberDataSet.duplicates[intCounter].Description;
                            NewDuplicateRow.JDEPartNumber = strJDEPartNumber;
                            NewDuplicateRow.Keep = false;
                            NewDuplicateRow.PartID = TheDuplicatePartNumberDataSet.duplicates[intCounter].PartID;
                            NewDuplicateRow.PartNumber = TheDuplicatePartNumberDataSet.duplicates[intCounter].PartNumber;
                            NewDuplicateRow.Remove = TheDuplicatePartNumberDataSet.duplicates[intCounter].Remove;

                            TheSearchedDuplicatePartNumbers.duplicates.Rows.Add(NewDuplicateRow);
                        }
                    }

                    dgrResults.ItemsSource = TheSearchedDuplicatePartNumbers.duplicates;
                    btnProcess.IsEnabled = true;
                }
                else
                {
                    btnProcess.IsEnabled = false;
                    dgrResults.ItemsSource = TheDuplicatePartNumberDataSet.duplicates;
                }
            }
            
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            //setting local variables
            int intPartID;
            bool blnFatalError = false;
            int intCounter;
            int intNumberOfRecords;
            int intSecondCounter;
            int intSecondNumberOfRecords;
            int intWarehouseID;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            try
            {
                //getting information
                intNumberOfRecords = TheSearchedDuplicatePartNumbers.duplicates.Rows.Count - 1;

                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    if(TheSearchedDuplicatePartNumbers.duplicates[intCounter].Keep == true)
                    {
                        if(TheSearchedDuplicatePartNumbers.duplicates[intCounter].Remove == false)
                        {
                            intPartID = TheSearchedDuplicatePartNumbers.duplicates[intCounter].PartID;

                            TheKeptWarehouseInventoryDataSet = TheInventoryClass.GetWarehouseCompleteInventoryByPartID(intPartID);
                        }
                    }
                }

                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    if(TheSearchedDuplicatePartNumbers.duplicates[intCounter].Remove == true)
                    {
                        if(TheSearchedDuplicatePartNumbers.duplicates[intCounter].Keep == false)
                        {
                            intPartID = TheSearchedDuplicatePartNumbers.duplicates[intCounter].PartID;

                            TheRemovedWarehouseInventoryDataSet = TheInventoryClass.GetWarehouseCompleteInventoryByPartID(intPartID);

                            intSecondNumberOfRecords = TheRemovedWarehouseInventoryDataSet.WarehouseInventory.Rows.Count - 1;

                            if(intSecondNumberOfRecords > -1)
                            {
                                for(intSecondCounter = 0; intSecondCounter <= intSecondNumberOfRecords; intSecondCounter++)
                                {
                                    intWarehouseID = TheRemovedWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].WarehouseID;
                                    blnFatalError = SetTransactionTables(intPartID, intWarehouseID);
                                    if(blnFatalError == false)
                                    {
                                        blnFatalError = UpdateWarehouseInventoryCount();

                                        if(blnFatalError == false)
                                        {
                                            blnFatalError = ThePartNumberClass.RemovePartFromPartList(TheRemovedWarehouseInventoryDataSet.WarehouseInventory[intSecondCounter].TablePartID);

                                            if(blnFatalError == true)
                                            {
                                                throw new Exception(gstrErrorMessage);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception(gstrErrorMessage);
                                    }
                                }
                            }

                            
                        }
                    }
                }
            }
            catch(Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Find Duplicate Part Numbers \\ Find Duplicate JDE Part Numbers \\ Process Button " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

            PleaseWait.Close();
            TheMessagesClass.InformationMessage("Process Complete");
        }
        private bool UpdateWarehouseInventoryCount()
        {
            bool blnFatalError = false;
            int intRemoveCounter;
            int intRemoveNumberOfRecords;
            int intKeptCounter;
            int intKeptNumberOfRecords;
            int intQuantity;
            int intTablePartID;
            int intWarehouseID;
            int intPartID;
            int intPartIDKept;
            bool blnItemNotFound;

            try
            {
                intRemoveNumberOfRecords = TheRemovedWarehouseInventoryDataSet.WarehouseInventory.Rows.Count - 1;
                intKeptNumberOfRecords = TheKeptWarehouseInventoryDataSet.WarehouseInventory.Rows.Count - 1;
                intPartIDKept = TheKeptWarehouseInventoryDataSet.WarehouseInventory[0].TablePartID;

                for(intRemoveCounter = 0; intRemoveCounter <= intRemoveNumberOfRecords; intRemoveCounter++)
                {
                    intTablePartID = TheRemovedWarehouseInventoryDataSet.WarehouseInventory[intRemoveCounter].TablePartID;
                    intWarehouseID = TheRemovedWarehouseInventoryDataSet.WarehouseInventory[intRemoveCounter].WarehouseID;
                    intQuantity = TheRemovedWarehouseInventoryDataSet.WarehouseInventory[intRemoveCounter].QTYOnHand;
                    intPartID = TheRemovedWarehouseInventoryDataSet.WarehouseInventory[intRemoveCounter].PartID;
                    blnItemNotFound = true;

                    for(intKeptCounter = 0; intKeptCounter <= intKeptNumberOfRecords; intKeptCounter++)
                    {
                        if(intWarehouseID == TheKeptWarehouseInventoryDataSet.WarehouseInventory[intKeptCounter].WarehouseID)
                        {
                            TheKeptWarehouseInventoryDataSet.WarehouseInventory[intKeptCounter].QTYOnHand += intQuantity;
                            blnItemNotFound = false;
                        }
                    }

                    if(blnItemNotFound == true)
                    {
                        blnFatalError =TheInventoryClass.UpdateWarehouseInventoryTablePartID(intPartID, intPartIDKept);
                    }
                    else
                    {
                        blnFatalError = TheInventoryClass.RemoveWarehouseInventoryRowByPartID(intPartID);
                    }
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Find Duplicate Part Numbers \\ Find Duplicate JDE Part Numbers \\ Update Warehouse Inventory " + Ex.Message);
            }

            return blnFatalError;
        }
        private bool SetTransactionTables(int intPartID, int intWarehouseID)
        {
            bool blnFatalError = false;
            int intPartIDKept;
            int intNumberOfRecords;
            int intCounter;
            int intTransactionID;

            try
            {
                intPartIDKept = TheKeptWarehouseInventoryDataSet.WarehouseInventory[0].TablePartID;

                //doing received parts
                TheReceivedPartsDataSet = TheReceivedMaterialClass.GetReceivedPartsByPartID(intPartID, intWarehouseID);

                intNumberOfRecords = TheReceivedPartsDataSet.ReceivedParts.Rows.Count - 1;

                if(intNumberOfRecords > -1)
                {
                    for(intCounter = 0; intCounter<= intNumberOfRecords; intCounter++)
                    {
                        intTransactionID = TheReceivedPartsDataSet.ReceivedParts[intCounter].TransactionID;
                        TheReceivedMaterialClass.UpdateReceivedPartsPartID(intPartIDKept, intTransactionID);
                    }
                }

                //doing issued parts
                TheIssuedPartsDataSet = TheIssuedPartsClass.GetIssuedPartsByPartID(intPartID, intWarehouseID);

                intNumberOfRecords = TheIssuedPartsDataSet.IssuedParts.Rows.Count - 1;

                if (intNumberOfRecords > -1)
                {
                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        intTransactionID = TheIssuedPartsDataSet.IssuedParts[intCounter].TransactionID;
                        TheIssuedPartsClass.UpdateIssuedPartsPartID(intPartIDKept, intTransactionID);
                    }
                }

                //doing bom parts
                TheBOMPartsDataSet = TheBOMPartsClass.GetBOMPartsByPartID(intPartID, intWarehouseID);

                intNumberOfRecords = TheBOMPartsDataSet.BOMParts.Rows.Count - 1;

                if (intNumberOfRecords > -1)
                {
                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        intTransactionID = TheBOMPartsDataSet.BOMParts[intCounter].TransactionID;
                        TheBOMPartsClass.UpdateBOMPartID(intTransactionID, intPartIDKept);
                    }
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Find Duplicate Part Numbers \\ Find Duplicate JDE Part Numbers \\ Set Transaction Tables " + Ex.Message);

                gstrErrorMessage = Ex.ToString();

                blnFatalError = true;
            }

            return blnFatalError;
        }
    }
}
