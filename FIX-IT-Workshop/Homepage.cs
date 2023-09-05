using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FIX_IT_Workshop
{
    public partial class Homepage : Form
    {
        Color selectedLabelColour = Color.FromArgb(180, 184, 171);
        Label currentlySelectedLabel;

        //Declare SqlControls
        private SqlConnection conn;
        private SqlCommand command;
        private SqlDataReader dataReader;
        private SqlDataAdapter dataAdapter;
        private DataSet dataSet;

        int userId;
        int repairClientId;
        int purchaseClientId;
        int customerPrimaryKey = -1;
        bool isAdmin;
        List<string> standartServiceItems = new List<string>();
        List<int> standartServiceItemQuantity = new List<int>();
        List<string> repairItems = new List<string>();
        List<int> repairItemsQuantity = new List<int>();
        List<string> purchaseItems = new List<string>();
        List<int> purchaseItemsQuantity = new List<int>();
        List<decimal> purchaseItemsPrice = new List<decimal>();


        //Declare connectionString global
        public String connectionString;

        public Homepage(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            standartServiceItems.Add("Spark Plug");
            standartServiceItems.Add("Brake Fluid");
            standartServiceItems.Add("Oil Filters");
            standartServiceItems.Add("Tires (4)");
            standartServiceItems.Add("Cabin Air Filter");
            standartServiceItems.Add("Air Filters");

            standartServiceItemQuantity.Add(1);
            standartServiceItemQuantity.Add(1);
            standartServiceItemQuantity.Add(1);
            standartServiceItemQuantity.Add(2);
            standartServiceItemQuantity.Add(3);
            standartServiceItemQuantity.Add(2);




        }

        private bool getAdminStatus(int userId)
        {
            string role = getValueInTable($"SELECT User_Role FROM [User] WHERE User_ID = {userId}", 0);

            return role.Trim().ToUpper() == "ADMIN";
        }

        private void connectDatabase()
        {
            //Initialize connectionString
            //connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\FixItDatabase.mdf;Integrated Security=True";

            connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\FixItDatabase.mdf;Integrated Security=True";

            try
            {
                //Create new Sql Connection
                conn = new SqlConnection(connectionString);

                //Open and Close the database to test for a successfull connection
                conn.Open();
                conn.Close();
            }
            catch (SqlException sqlException)
            {
                //Display appropiate message to the user
                MessageBox.Show("An error has occurred. Please try again later.");
                //Write error to console for debuging
                Console.WriteLine($"SqlError: {sqlException.Message}");
            }

        }

        public void resetAllLabels()
        {
            lblAddCustomer.ForeColor = Color.White;
            lblSupplier.ForeColor = Color.White;
            lblShop.ForeColor = Color.White;
            lblStock.ForeColor = Color.White;
            lblLogOut.ForeColor = Color.White;
            lblOrders.ForeColor = Color.White;
            lblUsers.ForeColor = Color.White;
        }

        public void selectLabel(Label selectedLabel)
        {
            resetAllLabels();
            selectedLabel.ForeColor = selectedLabelColour;
            currentlySelectedLabel = selectedLabel;
        }

        public void deselectLabel(Label selectedLabel)
        {
            if (currentlySelectedLabel != selectedLabel)
            {
                selectedLabel.ForeColor = Color.White;
            }
        }

        private void changeHeading(string headerText, string subHeaderText)
        {
            lblHeading.Text = headerText;
            lblSubHeading.Text = subHeaderText;
        }


        private void label7_Click(object sender, EventArgs e)
        {
            changeHeading("Manage customers", "Select an applicable option");
            tbcHomepage.SelectedTab = tbpAddCustomer;
            btnDeleteCustomer.Enabled = false;
            showNewCustomerPanel(pnlCustomerOptions);
            selectLabel(lblAddCustomer);
        }

        public void logoutUser()
        {
            Form1 loginPage = new Form1();
            loginPage.Show();

            this.Close();
        }

        public void confirmLogout()
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                logoutUser();
            }
        }

        private void lblLogOut_Click(object sender, EventArgs e)
        {
            changeHeading("Logout", "Confirm logout");
            tbcHomepage.SelectedIndex = -1;
            selectLabel(lblLogOut);
            confirmLogout();
        }

        private void lblShop_Click(object sender, EventArgs e)
        {
            changeHeading("Sales", "Select an applicable option to manage sales");
            tbcHomepage.SelectedTab = tbpShop;
            showNewSalesPanel(pnlSaleChoice);
            selectLabel(lblShop);

            if (!isAdmin)
            {
                btnNavOrderFromSupplier.Enabled = false;

            }
            else
            {
                btnNavOrderFromSupplier.Enabled = true;
            }

        }

        private void lblBookings_Click(object sender, EventArgs e)
        {
            if (!isAdmin)
            {
                MessageBox.Show("You do no have permission to view this page contents", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            else
            {

                changeHeading("Suppliers", "Select an applicable option");

                tbcHomepage.SelectedTab = tbpBookings;
                selectLabel(lblSupplier);
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                dataAdapter = new SqlDataAdapter();
                dataSet = new DataSet();
                string sql = "SELECT Name,Contact_Number,Email FROM Supplier";
                command = new SqlCommand(sql, conn);
                dataAdapter.SelectCommand = command;
                dataAdapter.Fill(dataSet, "Supplier");
                dgvSupp.DataSource = dataSet;
                dgvSupp.DataMember = "Supplier";
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

        }

        private void lblOrders_Click(object sender, EventArgs e)
        {
            if (!isAdmin)
            {
                MessageBox.Show("You do no have permission to view this page contents", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            else
            {

                tbcHomepage.SelectedTab = tbpOrders;
                selectLabel(lblOrders);
                changeHeading("Reports", "Select a category to generate a report");

                dgvBestSellingProductsSummary.Visible = true;
                crtBestSellingProducts.Visible = false;

                showReports(pnlReportsBestInventory);


                populateChart(5);
                populateDataGridView(5);
            }
        }

        private void lblStock_Click(object sender, EventArgs e)//Zohan
        {
            changeHeading("Stock", "Select an applicable option");
            try
            {
                tbcHomepage.SelectedTab = tbpStock;
                selectLabel(lblStock);
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                dataAdapter = new SqlDataAdapter();
                dataSet = new DataSet();

                string query = @"SELECT  Inventory.Product_Number,  Inventory.Product_Name, Inventory.Available_Quantity, Inventory.Unit_Price, Supplier.Name AS Supplier_Name FROM Inventory INNER JOIN Supplier ON Inventory.Supplier_ID = Supplier.Supplier_ID";

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                dgvProductStock.DataSource = dataTable;
                string CommandStock = @"SELECT Name FROM Supplier";
                SqlCommand cmd = new SqlCommand(CommandStock, conn); ;
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    cbxSupplierStock.Items.Add(dataReader.GetValue(0));

                }

                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            catch (SqlException sqlEx)
            {

                MessageBox.Show(sqlEx.ToString());
            }
            catch (Exception Ex)
            {

                MessageBox.Show(Ex.ToString());
            }
        }

        private void lblAddCustomer_MouseEnter(object sender, EventArgs e)
        {
            selectLabel(lblAddCustomer);
        }

        private void lblAddCustomer_MouseLeave(object sender, EventArgs e)
        {
            deselectLabel(lblAddCustomer);
        }

        private void lblShop_MouseEnter(object sender, EventArgs e)
        {
            selectLabel(lblShop);
        }

        private void lblShop_MouseLeave(object sender, EventArgs e)
        {
            deselectLabel(lblShop);
        }

        private void lblBookings_MouseEnter(object sender, EventArgs e)
        {
            selectLabel(lblSupplier);
        }

        private void lblBookings_MouseLeave(object sender, EventArgs e)
        {
            deselectLabel(lblSupplier);
        }

        private void lblOrders_MouseEnter(object sender, EventArgs e)
        {
            selectLabel(lblOrders);
        }

        private void lblOrders_MouseLeave(object sender, EventArgs e)
        {
            deselectLabel(lblShop);
        }

        private void lblStock_MouseEnter(object sender, EventArgs e)
        {
            selectLabel(lblStock);
        }

        private void lblStock_MouseLeave(object sender, EventArgs e)
        {
            deselectLabel(lblStock);
        }

        private void lblLogOut_MouseEnter(object sender, EventArgs e)
        {
            selectLabel(lblLogOut);
        }

        private void lblLogOut_MouseLeave(object sender, EventArgs e)
        {
            selectLabel(lblLogOut);
        }

        public void clearCustomerDetailValues()
        {
            txtCustomerFirstName.Clear();
            txtCustomerLastName.Clear();
            txtCustomerEmail.Clear();
            txtCustomerContactNumber.Clear();

        }

        public void clearCustomerVehcileDetails()
        {
            txtCustomerVehicleMake.Clear();
            txtCustomerVehicleModel.Clear();
            txtCustomerVehicleYear.Clear();
            txtCustomerVehicleLicensePlate.Clear();
        }

        public bool verifyVehicleDetails()
        {
            //Check if all textfields are populated
            if (txtCustomerVehicleMake.Text == "" || txtCustomerVehicleModel.Text == "" || txtCustomerVehicleYear.Text == "" || txtCustomerVehicleLicensePlate.Text == "")
            {
                return false;
            }

            //Return bool value based on user input
            return true;
        }

        public bool verifyCustomerDetails()
        {
            //Check if all textfields are populated
            if (txtCustomerFirstName.Text == "" || txtCustomerLastName.Text == "" || txtCustomerEmail.Text == "" || txtCustomerContactNumber.Text == "")
            {
                return false;
            }

            //Return bool value based on user input
            return true;
        }

        private void btnAddNewCustomer_Click_1(object sender, EventArgs e)
        {
            showNewCustomerPanel(pnlCustomerDetails);
            pnlCustomerDetails.BringToFront();
        }

        private void btnCustomerDetailsContinue_Click(object sender, EventArgs e)
        {
            if (!validateEmail(txtCustomerEmail))
            {
                MessageBox.Show($"Please enter a valid email before continuing.");
            }
            else if (txtCustomerContactNumber.Text.Length != 10)
            {
                MessageBox.Show("Please enter a valid contact number");
            }
            else if (verifyCustomerDetails())
            {
                showNewCustomerPanel(pnlCustomerVehicleInfo);
                pnlCustomerVehicleInfo.BringToFront();
            }
            else
            {
                //Display appropiate message to the user
                MessageBox.Show($"Please fill out all fields before continuing.");
            }
        }

        private void btnCancelAddCustomer_Click(object sender, EventArgs e)
        {
            clearCustomerDetailValues();
            showNewCustomerPanel(pnlCustomerOptions);
            pnlCustomerOptions.BringToFront();
        }

        private void addClient(string firstName, string lastName, string email, string contactNumber)
        {
            try
            {
                //Assign new connection
                conn = new SqlConnection(connectionString);

                //Open Connection
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                //Initialize new command
                string sql = $"INSERT INTO Client (First_Name, Last_Name, Email, Contact_Number) VALUES (@first_name, @last_name, @email, @contact_number)";
                command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@first_name", firstName);
                command.Parameters.AddWithValue("@last_name", lastName);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@contact_number", contactNumber);

                command.ExecuteNonQuery();

                //Display appropiate message to the user
                MessageBox.Show($"{firstName} {lastName} has been successfully registered.");

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }


            }
            catch (SqlException sqlException)
            {
                //Show suitable error message
                MessageBox.Show("Sign up failed.\nPlease try again later. \n" + sqlException.Message);

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                Console.WriteLine($"Error: {sqlException.Message}");
            }
        }

        private void addVehicle(string make, string model, string year, string licensePlateNumber, int customerId)
        {
            try
            {
                //Assign new connection
                conn = new SqlConnection(connectionString);

                //Open Connection
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                //Initialize new command
                string sql = $"INSERT INTO Vehicle (Make, Model, Year, License_Plate_Number) VALUES (@make, @model, @year, @lisence_plate_number)";
                command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@make", make);
                command.Parameters.AddWithValue("@model", model);
                command.Parameters.AddWithValue("@year", year);
                command.Parameters.AddWithValue("@lisence_plate_number", licensePlateNumber);
                command.ExecuteNonQuery();



                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            catch (SqlException sqlException)
            {
                //Show suitable error message
                MessageBox.Show("Sign up failed.\nPlease try again later." + sqlException.Message);

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                Console.WriteLine($"Error: {sqlException.Message}");
            }
        }




        public void resetCustomerViewAllFilter()
        {
            txtCustomerFirstNameFilter.Clear();
            txtCustomerLastNameFilter.Clear();
            txtCustomerEmailFilter.Clear();
            txtCustomerContactNumberFilter.Clear();
        }

        private void btnClearCustomerFilterFields_Click(object sender, EventArgs e)
        {
            resetCustomerViewAllFilter();
        }

        public void deleteRecord(string sql, bool showMessage)
        {
            try
            {


                //Open Connection
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                //Initialize new command
                command = new SqlCommand(sql, conn);

                //Initialzie dataAdapter
                dataAdapter = new SqlDataAdapter();

                //Execute statement
                dataAdapter.DeleteCommand = command;
                dataAdapter.DeleteCommand.ExecuteNonQuery();

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                if (showMessage)
                {
                    //Show suitable success message
                    MessageBox.Show($"Record successfully deleted");
                }
            }
            catch (SqlException sqlException)
            {
                //Show suitable error message
                MessageBox.Show("Failed to delete record.\nPlease try again later. " + sqlException.Message);

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                Console.WriteLine($"Error: {sqlException.Message}");
            }
        }

        public void resetDeleteCustomerFilter()
        {
            txtDeleteCustomerFirstName.Clear();
            txtDeleteCustomerLastName.Clear();
            txtDeleteCustomerEmail.Clear();
            txtDeleteCustomerContactNumber.Clear();
        }

        private void btnDeleteCustomer_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to delete this record? This cannout be undone.", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                string name = txtDeleteCustomerFirstName.Text;
                string surname = txtDeleteCustomerLastName.Text;
                string contactNumber = txtDeleteCustomerContactNumber.Text;
                string email = txtDeleteCustomerEmail.Text;
                setCustomerPrimaryKey(name, surname, email, contactNumber);
                deleteRecord($"DELETE FROM Client WHERE First_Name = '{txtDeleteCustomerFirstName.Text}' AND Last_Name = '{txtDeleteCustomerLastName.Text}' AND Contact_Number = '{txtDeleteCustomerContactNumber.Text}' AND Email = '{txtDeleteCustomerEmail.Text}'", false);
                deleteRecord($"DELETE FROM Vehicle WHERE Customer_ID = {customerPrimaryKey}", true);
                resetDeleteCustomerFilter();
            }
        }

        private void btnDeleteCustomersBack_Click(object sender, EventArgs e)
        {
            showNewCustomerPanel(pnlCustomerOptions);
            pnlCustomerOptions.BringToFront();
        }

        private void btnViewAllCancel_Click(object sender, EventArgs e)
        {
            showNewCustomerPanel(pnlCustomerOptions);
            pnlCustomerOptions.BringToFront();
        }

        private void filterRecords(string sql, DataGridView dataGridView)
        {
            try
            {
                // Open connection to the DB
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                // Initialize new Sql command
                command = new SqlCommand(sql, conn);

                // Initialize new Data Adapter
                dataAdapter = new SqlDataAdapter();

                // Declare and initialize new data Set
                DataSet dataSet = new DataSet();

                // Assign select command
                dataAdapter.SelectCommand = command;

                // Populate the dataset
                dataAdapter.Fill(dataSet, "Filter");

                // Assign datasource to dataset
                dataGridView.DataSource = dataSet;
                // Assign suitable Datamember "Movies"
                dataGridView.DataMember = "Filter";

                // Close conenction to DB
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                // Display suitable error dialog
                MessageBox.Show("An error has occured " + ex.Message);

                // Close connection if open
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void executeDisplaySql(string sql, DataGridView dataGrid)
        {
            try
            {
                // Open connection to the DB
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                // Initialize new Sql zommand
                command = new SqlCommand(sql, conn);

                // Initialize new Data Adapter
                dataAdapter = new SqlDataAdapter();

                // Declare and initialize new data Set
                DataSet dataSet = new DataSet();

                // Assign select command
                dataAdapter.SelectCommand = command;

                // Populate the dataset
                dataAdapter.Fill(dataSet, "SqlCommand");

                // Assign datasource to dataset
                dataGrid.DataSource = dataSet;
                // Assign suitable Datamember "Movies"
                dataGrid.DataMember = "SqlCommand";

                // Close conenction to DB
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                // Display suitable error dialog
                MessageBox.Show("An error has occured " + ex.Message);

                // Close connection if open
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void btnViewAllCustomers_Click(object sender, EventArgs e)
        {
            showNewCustomerPanel(pnlCustomerViewAll);
            executeDisplaySql($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client", dgvViewAllCustomers);
            pnlCustomerViewAll.BringToFront();
        }

        private void btnRemoveCustomer_Click(object sender, EventArgs e)
        {
            showNewCustomerPanel(pnlDeleteCustomer);
            executeDisplaySql($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client", dgvDeleteCustomer);
            pnlDeleteCustomer.BringToFront();
        }

        private void btnViewAllVehiclesCancel_Click(object sender, EventArgs e)
        {
            showNewCustomerPanel(pnlCustomerOptions);
            pnlCustomerOptions.BringToFront();
        }

        public void clearCustomerVehcileViewAllFilter()
        {
            txtViewAllVehiclesLicensePlate.Clear();
            txtViewAllVehiclesMake.Clear();
            txtViewAllVehiclesModel.Clear();
            txtViewAllVehiclesYear.Clear();
        }

        private void btnViewAllVehiclesClearFilter_Click(object sender, EventArgs e)
        {
            clearCustomerVehcileViewAllFilter();
        }

        private void btnViewAllVehicles_Click(object sender, EventArgs e)
        {
            showNewCustomerPanel(pnlViewAllVehicles);
            executeDisplaySql($"SELECT Make, Model, Year, License_Plate_Number FROM Vehicle", dgvViewAllVehicles);
            pnlViewAllVehicles.BringToFront();
        }

        private void btnUpdateCustomerDetailsCancel_Click(object sender, EventArgs e)
        {

            showNewCustomerPanel(pnlCustomerOptions);
            pnlCustomerOptions.BringToFront();
        }

        public void clearUpdateCustomerDetailsFilter()
        {
            txtUpdateCustomerFirstName.Clear();
            txtUpdateCustomerLastName.Clear();
            txtUpdateCustomerEmail.Clear();
            txtUpdateCustomerContactNumber.Clear();
        }

        private void btnUpdateCustomerDaetailsFilter_Click(object sender, EventArgs e)
        {
            clearUpdateCustomerDetailsFilter();
        }

        private void btnUpdateCustomerDetails_Click(object sender, EventArgs e)
        {
            btnUpdateCustomerDetailsConfirm.Enabled = false;
            showNewCustomerPanel(pnlUpdateCustomerDetails);
            executeDisplaySql($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client", dgvUpdateCustomerDetails);
            pnlUpdateCustomerDetails.BringToFront();
        }

        private void label6_Click(object sender, EventArgs e)
        {
            if (!isAdmin)
            {
                btnRemove_Users.Enabled = false;
                btnAdd_New_Users.Enabled = false;

            }
            else
            {
                btnRemove_Users.Enabled = true;
                btnAdd_New_Users.Enabled = true;
            }
            changeHeading("Users", "Select applicable option");
            tbcHomepage.SelectedTab = tbpUsers;
            showNewUserPanel(pnlUsers);
            selectLabel(lblUsers);
        }

        private void showNewCustomerPanel(Panel selectedPanel)
        {
            pnlCustomerOptions.Visible = false;
            pnlUpdateCustomerDetails.Visible = false;
            pnlCustomerDetails.Visible = false;
            pnlViewAllVehicles.Visible = false;
            pnlDeleteCustomer.Visible = false;
            pnlCustomerViewAll.Visible = false;
            pnlCustomerVehicleInfo.Visible = false;
            pnlUpdateCustomerDetailsFilled.Visible = false;
            pnlUpdateCustomerVehicleDetailsFilled.Visible = false;

            selectedPanel.Visible = true;
        }

        private void showReports(Panel selectedPanel)
        {
            pnlReportsBestInventory.Visible = false;

            selectedPanel.Visible = true;
        }

        private void showNewUserPanel(Panel selected_userPanel)
        {
            //Add jou eie panels wat op Users is
            pnlView_All_Users_panel.Visible = false;
            pnlAdd_New_Users.Visible = false;
            pnlUpdate_User_Details.Visible = false;
            pnlRemove_Users.Visible = false;
            pnlUsers.Visible = false;

            selected_userPanel.Visible = true;
        }

        private void Homepage_Load(object sender, EventArgs e)
        {
            connectDatabase();
            changeHeading("Manage customers", "Select an applicable option");
            tbcHomepage.SelectedTab = tbpAddCustomer;
            showNewCustomerPanel(pnlCustomerOptions);
            selectLabel(lblAddCustomer);

            isAdmin = getAdminStatus(this.userId);
        }

        private void cBUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*if (cBUsers.SelectedIndex != -1)
            {
                if (cBUsers.SelectedValue == "Users")
                {
                    //Display all the data of the users
                }
                if (cBUsers.SelectedValue == "Mechanical Technicians")
                {
                    //Display all the data of the mechanical technicians
                }
            }*/
        }

        private void txtFirst_Name_TextChanged(object sender, EventArgs e)
        {
            //Live filter the database with the FirstName
        }

        private void txtLast_Name_TextChanged(object sender, EventArgs e)
        {
            //Live filter the database with the LastName
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtFirst_Name_View_All_Users_panel.Clear();
            txtLast_Name_View_All_Users_panel.Clear();

            cBUserType_View_All_Users_panel.SelectedIndex = -1;
        }

        private void btnView_All_Users_Click_1(object sender, EventArgs e)
        {
            //tbcHomepage.SelectedTab = tbpUsers;
            showNewUserPanel(pnlUsers);
            pnlUsers.BringToFront();
        }

        //User Buttons

        private void btnView_All_Users_Click_2(object sender, EventArgs e)
        {
            showNewUserPanel(pnlView_All_Users_panel);
            executeDisplaySql("SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User]", dGVDisplay_Users_View_All_Users_panel);
            pnlView_All_Users_panel.BringToFront();
        }

        private void btnRemove_Users_Click_1(object sender, EventArgs e)
        {
            showNewUserPanel(pnlRemove_Users);
            pnlRemove_Users.BringToFront();
        }

        private void btnUpdate_User_Details_Click_1(object sender, EventArgs e)
        {

            showNewUserPanel(pnlRemove_Users);
            executeDisplaySql("SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User]", dGV_pnlRemove_Users_Display);
            pnlRemove_Users.BringToFront();
            btnRemove_User_panel.Enabled = false;
        }

        private void btnAdd_New_Users_Click_1(object sender, EventArgs e)
        {
            showNewUserPanel(pnlAdd_New_Users);
            executeDisplaySql("SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User]", dGV_Add_New_Users_panel);
            pnlAdd_New_Users.BringToFront();
        }

        private void btnCancel_View_All_Users_panel_Click(object sender, EventArgs e)
        {
            showNewUserPanel(pnlUsers);
        }

        private void btnCancel_Update_User_Details_panel_Click(object sender, EventArgs e)
        {
            showNewUserPanel(pnlUsers);
        }

        private void btnCancel_on_RemoveUser_panel_Click(object sender, EventArgs e)
        {
            showNewUserPanel(pnlUsers);
        }

        private void btn_Cancel_AddUsers_panel_Click(object sender, EventArgs e)
        {
            showNewUserPanel(pnlUsers);
        }

        private void btnAddSupp_Click(object sender, EventArgs e)
        {

            if (!validateEmail(tbEmailSupp))
            {
                MessageBox.Show("Please enter a valid email");
            }
            else if (tbCNumberSupp.Text.Length != 10)
            {
                MessageBox.Show("Please enter a valid contact number");
            }
            else
            {
                try
                {

                    //Open Connection
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    dataSet = new DataSet();
                    string sql = $"INSERT INTO Supplier(Name,Contact_Number,Email) VALUES ('{tbNameSupp.Text}','{tbCNumberSupp.Text}','{tbEmailSupp.Text}' )";
                    command = new SqlCommand(sql, conn);
                    SqlDataAdapter adap = new SqlDataAdapter();
                    adap.InsertCommand = command;
                    adap.InsertCommand.ExecuteNonQuery();
                    // conn.Close();
                    tbCNumberSupp.Clear();
                    tbEmailSupp.Clear();
                    tbNameSupp.Clear();
                    // conn.Open();
                    adap = new SqlDataAdapter();
                    dataSet = new DataSet();
                    sql = "SELECT Name,Contact_Number,Email FROM Supplier";
                    command = new SqlCommand(sql, conn);
                    adap.SelectCommand = command;
                    adap.Fill(dataSet, "Supplier");
                    dgvSupp.DataSource = dataSet;
                    dgvSupp.DataMember = "Supplier";
                    //Open Connection
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
                catch (SqlException sqlex)
                {
                    MessageBox.Show(sqlex.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            conn = new SqlConnection(connectionString);
            conn.Open();
            dataAdapter = new SqlDataAdapter();
            dataSet = new DataSet();
            string sql = $"SELECT Name, Email, Contact_Number FROM Supplier WHERE UPPER(Name) LIKE '%{tbNameSupp.Text.ToUpper()}%'";
            command = new SqlCommand(sql, conn);
            dataAdapter.SelectCommand = command;
            dataAdapter.Fill(dataSet, "Supplier");
            conn.Close();
        }

        private void btnDeleteSupp_Click(object sender, EventArgs e)
        {
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                dataSet = new DataSet();
                string sql = $"DELETE FROM Supplier Where Name ='{dgvSupp[0, dgvSupp.CurrentRow.Index].Value}'";

                command = new SqlCommand(sql, conn);
                SqlDataAdapter adap = new SqlDataAdapter();
                adap.InsertCommand = command;
                adap.InsertCommand.ExecuteNonQuery();
                // conn.Close();
                tbCNumberSupp.Clear();
                tbEmailSupp.Clear();
                tbNameSupp.Clear();
                // conn.Open();
                adap = new SqlDataAdapter();
                dataSet = new DataSet();
                sql = "SELECT Name,Contact_Number,Email FROM Supplier";
                command = new SqlCommand(sql, conn);
                adap.SelectCommand = command;
                adap.Fill(dataSet, "Supplier");
                dgvSupp.DataSource = dataSet;
                dgvSupp.DataMember = "Supplier";
                conn.Close();
            }
            catch (SqlException sqlex)
            {
                MessageBox.Show(sqlex.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnClearFilterSupp_Click(object sender, EventArgs e)
        {
            tbCNumberSupp.Clear();
            tbEmailSupp.Clear();
            tbNameSupp.Clear();
        }

        private void tbEmailSupp_TextChanged(object sender, EventArgs e)
        {

        }

        private void tbCNumberSupp_TextChanged(object sender, EventArgs e)
        {
        }
        private void sendEmail(string email, string receiptContent)
        {
            try
            {
                //Setup SMTP client for using gmail
                SmtpClient clientDetails = new SmtpClient("smtp.gmail.com");
                clientDetails.Port = 587;
                clientDetails.EnableSsl = true;
                clientDetails.DeliveryMethod = SmtpDeliveryMethod.Network;
                clientDetails.UseDefaultCredentials = false;
                clientDetails.Credentials = new NetworkCredential("fixitnwu@gmail.com", "ktktkgwnuuapipxy");

                //Actually filling in the contents of the message based on the generated receipt
                MailMessage mailDetails = new MailMessage();
                mailDetails.From = new MailAddress("fixitnwu@gmail.com");
                mailDetails.To.Add(email);
                mailDetails.Subject = $"Receipt from FIX IT Workshop";
                mailDetails.Body = receiptContent;

                //Send the email
                clientDetails.Send(mailDetails);


                MessageBox.Show("Email has been successfully been send to the customer!");
            }
            catch (Exception ex)
            {
                //Display message to user incase the email sending failed
                MessageBox.Show("Email could not be sent, please take a picture of receipt.");
                Console.WriteLine(ex.Message);
            }

        }

        private void txtDeleteCustomerFirstName_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client WHERE UPPER(First_Name) LIKE '%{txtDeleteCustomerFirstName.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtDeleteCustomerLastName.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{txtDeleteCustomerEmail.Text.ToUpper()}%' AND UPPER(Contact_Number) LIKE '%{txtDeleteCustomerContactNumber.Text.ToUpper()}%'", dgvDeleteCustomer);
        }

        private void txtDeleteCustomerLastName_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client WHERE UPPER(First_Name) LIKE '%{txtDeleteCustomerFirstName.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtDeleteCustomerLastName.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{txtDeleteCustomerEmail.Text.ToUpper()}%' AND UPPER(Contact_Number) LIKE '%{txtDeleteCustomerContactNumber.Text.ToUpper()}%'", dgvDeleteCustomer);
        }

        private void txtDeleteCustomerEmail_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client WHERE UPPER(First_Name) LIKE '%{txtDeleteCustomerFirstName.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtDeleteCustomerLastName.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{txtDeleteCustomerEmail.Text.ToUpper()}%' AND UPPER(Contact_Number) LIKE '%{txtDeleteCustomerContactNumber.Text.ToUpper()}%'", dgvDeleteCustomer);
        }

        private void txtDeleteCustomerContactNumber_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client WHERE UPPER(First_Name) LIKE '%{txtDeleteCustomerFirstName.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtDeleteCustomerLastName.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{txtDeleteCustomerEmail.Text.ToUpper()}%' AND UPPER(Contact_Number) LIKE '%{txtDeleteCustomerContactNumber.Text.ToUpper()}%'", dgvDeleteCustomer);
        }

        private void txtViewAllVehiclesMake_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT Make, Model, Year, License_Plate_Number FROM Vehicle WHERE UPPER(Make) LIKE '%{txtViewAllVehiclesMake.Text.ToUpper()}%' AND UPPER(Model) LIKE '%{txtViewAllVehiclesModel.Text.ToUpper()}%' AND UPPER(Year) LIKE '%{txtViewAllVehiclesYear.Text.ToUpper()}%' AND UPPER(License_Plate_Number) LIKE '%{txtViewAllVehiclesLicensePlate.Text.ToUpper()}%'", dgvViewAllVehicles);
        }

        private void txtViewAllVehiclesModel_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT Make, Model, Year, License_Plate_Number FROM Vehicle WHERE UPPER(Make) LIKE '%{txtViewAllVehiclesMake.Text.ToUpper()}%' AND UPPER(Model) LIKE '%{txtViewAllVehiclesModel.Text.ToUpper()}%' AND UPPER(Year) LIKE '%{txtViewAllVehiclesYear.Text.ToUpper()}%' AND UPPER(License_Plate_Number) LIKE '%{txtViewAllVehiclesLicensePlate.Text.ToUpper()}%'", dgvViewAllVehicles);
        }

        private void txtViewAllVehiclesLicensePlate_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT Make, Model, Year, License_Plate_Number FROM Vehicle WHERE UPPER(Make) LIKE '%{txtViewAllVehiclesMake.Text.ToUpper()}%' AND UPPER(Model) LIKE '%{txtViewAllVehiclesModel.Text.ToUpper()}%' AND UPPER(Year) LIKE '%{txtViewAllVehiclesYear.Text.ToUpper()}%' AND UPPER(License_Plate_Number) LIKE '%{txtViewAllVehiclesLicensePlate.Text.ToUpper()}%'", dgvViewAllVehicles);
        }

        private void txtViewAllVehiclesYear_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT Make, Model, Year, License_Plate_Number FROM Vehicle WHERE UPPER(Make) LIKE '%{txtViewAllVehiclesMake.Text.ToUpper()}%' AND UPPER(Model) LIKE '%{txtViewAllVehiclesModel.Text.ToUpper()}%' AND UPPER(Year) LIKE '%{txtViewAllVehiclesYear.Text.ToUpper()}%' AND UPPER(License_Plate_Number) LIKE '%{txtViewAllVehiclesLicensePlate.Text.ToUpper()}%'", dgvViewAllVehicles);
        }

        private void txtUpdateCustomerFirstName_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client WHERE UPPER(First_Name) LIKE '%{txtUpdateCustomerFirstName.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtUpdateCustomerLastName.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{txtUpdateCustomerEmail.Text.ToUpper()}%' AND UPPER(Contact_Number) LIKE '%{txtUpdateCustomerContactNumber.Text.ToUpper()}%'", dgvUpdateCustomerDetails);
        }

        private void txtUpdateCustomerLastName_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client WHERE UPPER(First_Name) LIKE '%{txtUpdateCustomerFirstName.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtUpdateCustomerLastName.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{txtUpdateCustomerEmail.Text.ToUpper()}%' AND UPPER(Contact_Number) LIKE '%{txtUpdateCustomerContactNumber.Text.ToUpper()}%'", dgvUpdateCustomerDetails);
        }

        private void txtUpdateCustomerEmail_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client WHERE UPPER(First_Name) LIKE '%{txtUpdateCustomerFirstName.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtUpdateCustomerLastName.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{txtUpdateCustomerEmail.Text.ToUpper()}%' AND UPPER(Contact_Number) LIKE '%{txtUpdateCustomerContactNumber.Text.ToUpper()}%'", dgvUpdateCustomerDetails);
        }

        private void txtUpdateCustomerContactNumber_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client WHERE UPPER(First_Name) LIKE '%{txtUpdateCustomerFirstName.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtUpdateCustomerLastName.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{txtUpdateCustomerEmail.Text.ToUpper()}%' AND UPPER(Contact_Number) LIKE '%{txtUpdateCustomerContactNumber.Text.ToUpper()}%'", dgvUpdateCustomerDetails);
        }

        private void txtCustomerFirstNameFilter_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client WHERE UPPER(First_Name) LIKE '%{txtCustomerFirstNameFilter.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtCustomerLastNameFilter.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{txtCustomerEmailFilter.Text.ToUpper()}%' AND UPPER(Contact_Number) LIKE '%{txtCustomerContactNumberFilter.Text.ToUpper()}%'", dgvViewAllCustomers);

        }

        private void txtCustomerLastNameFilter_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client WHERE UPPER(First_Name) LIKE '%{txtCustomerFirstNameFilter.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtCustomerLastNameFilter.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{txtCustomerEmailFilter.Text.ToUpper()}%' AND UPPER(Contact_Number) LIKE '%{txtCustomerContactNumberFilter.Text.ToUpper()}%'", dgvViewAllCustomers);

        }

        private void txtCustomerContactNumberFilter_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client WHERE UPPER(First_Name) LIKE '%{txtCustomerFirstNameFilter.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtCustomerLastNameFilter.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{txtCustomerEmailFilter.Text.ToUpper()}%' AND UPPER(Contact_Number) LIKE '%{txtCustomerContactNumberFilter.Text.ToUpper()}%'", dgvViewAllCustomers);
        }

        private void txtCustomerEmailFilter_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT First_Name, Last_Name, Email, Contact_Number FROM Client WHERE UPPER(First_Name) LIKE '%{txtCustomerFirstNameFilter.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtCustomerLastNameFilter.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{txtCustomerEmailFilter.Text.ToUpper()}%' AND UPPER(Contact_Number) LIKE '%{txtCustomerContactNumberFilter.Text.ToUpper()}%'", dgvViewAllCustomers);
        }

        private void showNewSalesPanel(Panel selectedPanel)
        {
            pnlSaleChoice.Visible = false;
            pnlServices.Visible = false;
            pnlRepair.Visible = false;
            pnlMakePurcahes.Visible = false;
            pnlViewTransaction.Visible = false;
            pnlChangeSale.Visible = false;
            pnlOrderStock.Visible = false;

            selectedPanel.Visible = true;
        }

        private void returnToSales()
        {
            showNewSalesPanel(pnlSaleChoice);
            pnlSaleChoice.BringToFront();
        }

        private void btnViewTransactionCancel_Click(object sender, EventArgs e)
        {
            returnToSales();
        }

        private void btnServiceCancel_Click(object sender, EventArgs e)
        {
            returnToSales();
        }

        private void btnPurchaseCancel_Click(object sender, EventArgs e)
        {
            returnToSales();
        }

        private void btnPurchaseMakePurchase_Click(object sender, EventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(txtPurchaseClientContactNum.Text))
                {
                    MessageBox.Show("Please fill out all client details before continuing");
                }
                else
                if (lbxPurchaseProduct.Items.Count < 3)
                {
                    MessageBox.Show("Please select an item before continuing");
                }
                else
                {

                    decimal totalPrice = 0;
                    foreach (var item in purchaseItemsPrice)
                    {
                        totalPrice += item;
                    }

                    addTransactionEntry(purchaseClientId, 0, totalPrice, userId, null, purchaseItems, purchaseItemsQuantity, "Sale");
                    MessageBox.Show("The transaction was successful.");
                    txtPurchaseClientContactNum.Clear();
                    returnToSales();
                }
            }
            catch (SqlException sqlex)
            {
                MessageBox.Show(sqlex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btnChangeSaleCancel_Click(object sender, EventArgs e)
        {
            returnToSales();
        }

        private void btnOrderStockCancel_Click(object sender, EventArgs e)
        {
            returnToSales();
        }

        private void btnRepairCancel_Click(object sender, EventArgs e)
        {
            returnToSales();
        }


        private void btnNavService_Click_1(object sender, EventArgs e)
        {
            showNewSalesPanel(pnlServices);
            pnlServices.BringToFront();
            executeDisplaySql("SELECT DISTINCT Transact.Status, Cust.First_Name, Cust.Last_Name FROM [Transaction] Transact JOIN Client Cust ON Transact.Client_ID = Cust.Client_ID JOIN Transaction_Item TransactItem ON Transact.Transaction_ID = TransactItem.Transaction_ID WHERE TransactItem.Transaction_Description = 'Service' AND Transact.Status <> 'Completed'", dagvBookingOfServices);

            dpBookServiceTime.MinDate = DateTime.Today.AddHours(8);
            dpBookServiceTime.MaxDate = DateTime.Today.AddHours(15);


            dpBookServiceTime.Value = DateTime.Today.AddHours(8);
        }

        private void btnNavOrderFromSupplier_Click_1(object sender, EventArgs e)
        {
            showNewSalesPanel(pnlOrderStock);
            pnlOrderStock.BringToFront();


            executeDisplaySql("SELECT Product_Number, Product_Name FROM Inventory", dgvOrderStock);
        }

        private void btnNavChangeSales_Click(object sender, EventArgs e)
        {
            showNewSalesPanel(pnlChangeSale);
            pnlChangeSale.BringToFront();

            filterRecords($"SELECT C.First_Name, C.Last_Name, T.Payment_Date, T.Total_Amount, TI.Transaction_Description FROM Client AS C INNER JOIN [Transaction] AS T ON C.Client_ID = T.Client_ID INNER JOIN Transaction_Item AS TI ON T.Transaction_ID = TI.Transaction_ID WHERE LEFT(C.First_Name, 1) LIKE '%%' AND LEFT(C.Last_Name, 1) LIKE '%%'", dgvChangeSaleGridView);
        }

        private void btnNavTransaction_Click_1(object sender, EventArgs e)
        {
            showNewSalesPanel(pnlViewTransaction);
            pnlViewTransaction.BringToFront();

            try
            {
                filterRecords($"SELECT  T.Payment_Date, C.First_Name, C.Last_Name, T.Total_Amount, TI.Transaction_Description FROM Client AS C INNER JOIN [Transaction] AS T ON C.Client_ID = T.Client_ID INNER JOIN Transaction_Item AS TI ON T.Transaction_ID = TI.Transaction_ID WHERE LEFT(C.First_Name, 1) LIKE '%%' AND LEFT(C.Last_Name, 1) LIKE '%%'", dgvViewTransactionGridView);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnNavRepair_Click_1(object sender, EventArgs e)
        {
            showNewSalesPanel(pnlRepair);
            pnlRepair.BringToFront();

            executeDisplaySql("SELECT Product_Name, Product_Number, Available_Quantity, Unit_Price FROM Inventory", dgvRepairsParts);
        }

        private void btnNavSell_Click_1(object sender, EventArgs e)
        {
            showNewSalesPanel(pnlMakePurcahes);
            pnlMakePurcahes.BringToFront();

            executeDisplaySql("SELECT Product_Number, Product_Name, Available_Quantity, Unit_Price FROM Inventory", dgvPurchaseGridView);
        }

        private void updateRecord(string sql)
        {
            try
            {
                //Open Connection
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                //Initialize new command
                command = new SqlCommand(sql, conn);

                //Initialzie dataAdapter
                dataAdapter = new SqlDataAdapter();

                //Execute statement
                dataAdapter.UpdateCommand = command;
                dataAdapter.UpdateCommand.ExecuteNonQuery();

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                //Show suitable success message
                MessageBox.Show($"Changes has successfully been saved.");
            }
            catch (SqlException sqlException)
            {
                //Show suitable error message
                MessageBox.Show("Failed to save changes.\nPlease try again later.");

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                Console.WriteLine($"Error: {sqlException.Message}");
            }
        }



        private void updateCustomerDetails(string sql)
        {
            // Check if any row is selected
            if (dgvUpdateCustomerDetails.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dgvUpdateCustomerDetails.SelectedRows[0];

                // Access the cell values from the selected row using column indexes
                string firstName = txtUpdateCustomerFirstName.Text;
                string lastName = txtUpdateCustomerLastName.Text;
                string email = txtUpdateCustomerEmail.Text;
                string contactNumber = txtUpdateCustomerContactNumber.Text;

                if (customerPrimaryKey != -1)
                {

                }
                else
                {
                    //Show suitable error message
                    MessageBox.Show("Failed to load client profile.\nPlease try again later.");
                }

            }
        }

        private void setCustomerPrimaryKey(string firstName, string lastName, string email, string contactNumber)
        {
            customerPrimaryKey = -1;
            try
            {
                // Open connection to the DB
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                //Count all matching emails to check for duplicates
                String sql = $"SELECT Client_ID FROM [Client] WHERE UPPER(Email)  = '{email.ToUpper()}' AND UPPER(First_Name)  = '{firstName.ToUpper()}' AND UPPER(Last_Name)  = '{lastName.ToUpper()}' AND Contact_Number  = '{contactNumber}'";

                // Initialize new Sql command
                command = new SqlCommand(sql, conn);

                // Execute command
                dataReader = command.ExecuteReader();



                while (dataReader.Read())
                {
                    customerPrimaryKey = dataReader.GetInt32(0);
                }
                // Close conenction to DB
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                // Display suitable error dialog
                MessageBox.Show("An error has occured " + ex.Message);

                // Close connection if open
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void setCustomerUpdateVehicleFields(int customerId)
        {

            try
            {
                // Open connection to the DB
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                //Count all matching emails to check for duplicates
                String sql = $"SELECT Make, Model, Year, License_Plate_Number FROM Vehicle WHERE Customer_ID = {customerId}";

                // Initialize new Sql command
                command = new SqlCommand(sql, conn);

                // Execute command
                dataReader = command.ExecuteReader();



                while (dataReader.Read())
                {
                    txtUpdateCustomerVehicleDetailsFilledMake.Text = dataReader.GetValue(0).ToString();
                    txtUpdateCustomerVehicleDetailsFilledModel.Text = dataReader.GetValue(1).ToString();
                    txtUpdateCustomerVehicleDetailsFilledYear.Text = dataReader.GetValue(2).ToString();
                    txtUpdateCustomerVehicleDetailsFilledLicensePlate.Text = dataReader.GetValue(3).ToString();
                }
                // Close conenction to DB
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                // Display suitable error dialog
                MessageBox.Show("An error has occured " + ex.Message);

                // Close connection if open
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }


        }


        private void btnUpdateCustomerDetailsConfirm_Click(object sender, EventArgs e)
        {
            if (!validateEmail(txtUpdateCustomerEmail))
            {
                MessageBox.Show("Please enter a valid email");
            }
            else if (txtUpdateCustomerContactNumber.Text.Length != 10)
            {
                MessageBox.Show("Please enter a valid contact number");
            }
            else
            {
                showNewCustomerPanel(pnlUpdateCustomerDetailsFilled);
                txtUpdateCustomerDetailsFilledFirstName.Text = txtUpdateCustomerFirstName.Text;
                txtUpdateCustomerDetailsFilledLastName.Text = txtUpdateCustomerLastName.Text;
                txtUpdateCustomerDetailsFilledEmail.Text = txtUpdateCustomerEmail.Text;
                txtUpdateCustomerDetailsFilledContactNumber.Text = txtUpdateCustomerContactNumber.Text;

                setCustomerPrimaryKey(txtUpdateCustomerDetailsFilledFirstName.Text, txtUpdateCustomerDetailsFilledLastName.Text, txtUpdateCustomerDetailsFilledEmail.Text, txtUpdateCustomerDetailsFilledContactNumber.Text);
                setCustomerUpdateVehicleFields(customerPrimaryKey);
            }



        }

        private void populateUpdateCustomerDetailsTextBoxes()
        {
            // Check if any row is selected
            if (dgvUpdateCustomerDetails.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dgvUpdateCustomerDetails.SelectedRows[0];

                // Access the cell values from the selected row using column indexes
                string firstName = selectedRow.Cells["First_Name"].Value.ToString();
                string lastName = selectedRow.Cells["Last_Name"].Value.ToString();
                string email = selectedRow.Cells["Email"].Value.ToString();
                string contactNumber = selectedRow.Cells["Contact_Number"].Value.ToString();

                txtUpdateCustomerContactNumber.Text = contactNumber;
                txtUpdateCustomerEmail.Text = email;
                txtUpdateCustomerFirstName.Text = firstName;
                txtUpdateCustomerLastName.Text = lastName;

                btnUpdateCustomerDetailsConfirm.Enabled = true;
            }

        }

        private void populateViewAllVehiclesTextBoxes()
        {
            // Check if any row is selected
            if (dgvViewAllVehicles.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dgvViewAllVehicles.SelectedRows[0];

                // Access the cell values from the selected row using column indexes
                string make = selectedRow.Cells["Make"].Value.ToString();
                string model = selectedRow.Cells["Model"].Value.ToString();
                string year = selectedRow.Cells["Year"].Value.ToString();
                string licensePlateNumber = selectedRow.Cells["License_Plate_Number"].Value.ToString();

                txtViewAllVehiclesMake.Text = make;
                txtViewAllVehiclesYear.Text = year;
                txtViewAllVehiclesModel.Text = model;
                txtViewAllVehiclesLicensePlate.Text = licensePlateNumber;
            }
        }

        private void populateViewAllCustomerTextBoxes()
        {
            // Check if any row is selected
            if (dgvViewAllCustomers.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dgvViewAllCustomers.SelectedRows[0];

                // Access the cell values from the selected row using column indexes
                string firstName = selectedRow.Cells["First_Name"].Value.ToString();
                string lastName = selectedRow.Cells["Last_Name"].Value.ToString();
                string email = selectedRow.Cells["Email"].Value.ToString();
                string contactNumber = selectedRow.Cells["Contact_Number"].Value.ToString();

                txtCustomerFirstNameFilter.Text = firstName;
                txtCustomerLastNameFilter.Text = lastName;
                txtCustomerEmailFilter.Text = email;
                txtCustomerContactNumberFilter.Text = contactNumber;
            }
        }

        private void populateDeleteCustomerTextBoxes()
        {
            // Check if any row is selected
            if (dgvDeleteCustomer.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dgvDeleteCustomer.SelectedRows[0];

                // Access the cell values from the selected row using column indexes
                string firstName = selectedRow.Cells["First_Name"].Value.ToString();
                string lastName = selectedRow.Cells["Last_Name"].Value.ToString();
                string email = selectedRow.Cells["Email"].Value.ToString();
                string contactNumber = selectedRow.Cells["Contact_Number"].Value.ToString();

                txtDeleteCustomerFirstName.Text = firstName;
                txtDeleteCustomerLastName.Text = lastName;
                txtDeleteCustomerContactNumber.Text = contactNumber;
                txtDeleteCustomerEmail.Text = email;

                btnDeleteCustomer.Enabled = true;
            }
            else
            {
                btnDeleteCustomer.Enabled = false;
            }

        }

        private void dgvUpdateCustomerDetails_SelectionChanged(object sender, EventArgs e)
        {
            populateUpdateCustomerDetailsTextBoxes();
        }

        private void btnUpdateCustomerDetailsFilledCancel_Click_1(object sender, EventArgs e)
        {
            showNewCustomerPanel(pnlUpdateCustomerDetails);
        }

        private void btnUpdateCustomerDetailsFilledContinue_Click(object sender, EventArgs e)
        {
            if (!validateEmail(txtUpdateCustomerDetailsFilledEmail))
            {
                MessageBox.Show($"Please enter a valid email before continuing.");
            }
            else if (txtUpdateCustomerDetailsFilledContactNumber.Text.Length != 10)
            {
                MessageBox.Show("Please enter a valid contact number");
            }
            else
            {
                showNewCustomerPanel(pnlUpdateCustomerVehicleDetailsFilled);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            showNewCustomerPanel(pnlUpdateCustomerDetails);
            string firstName = txtUpdateCustomerDetailsFilledFirstName.Text;
            string lastName = txtUpdateCustomerDetailsFilledLastName.Text;
            string email = txtUpdateCustomerDetailsFilledEmail.Text;
            string contactNumber = txtUpdateCustomerDetailsFilledContactNumber.Text;

            string make = txtUpdateCustomerVehicleDetailsFilledMake.Text;
            string model = txtUpdateCustomerVehicleDetailsFilledModel.Text;
            string year = txtUpdateCustomerVehicleDetailsFilledYear.Text;
            string licensePlate = txtUpdateCustomerVehicleDetailsFilledLicensePlate.Text;


            updateRecord($"UPDATE Client SET First_name = '{firstName}', Last_Name = '{lastName}', Email = '{email}', Contact_Number = '{contactNumber}' WHERE Client_ID = ${customerPrimaryKey}");
            updateRecord($"UPDATE Vehicle SET Make = '{make}', Model = '{model}', Year = '{year}', License_Plate_Number = '{licensePlate}' WHERE Customer_ID = ${customerPrimaryKey}");
        }

        private void btnUpdateCustomerVehicleDetailsFilledBack_Click(object sender, EventArgs e)
        {
            showNewCustomerPanel(pnlUpdateCustomerDetailsFilled);
        }

        private void btnCustomerVehicleInfoFinish_Click_1(object sender, EventArgs e)
        {

            if (verifyVehicleDetails())
            {
                string firstName = txtCustomerFirstName.Text;
                string lastName = txtCustomerLastName.Text;
                string email = txtCustomerEmail.Text;
                string contactNumber = txtCustomerContactNumber.Text;

                string make = txtCustomerVehicleMake.Text;
                string model = txtCustomerVehicleModel.Text;
                string year = txtCustomerVehicleYear.Text;
                string licensePlate = txtCustomerVehicleLicensePlate.Text;

                addClient(firstName, lastName, email, contactNumber);


                setCustomerPrimaryKey(firstName, lastName, email, contactNumber);
                addVehicle(make, model, year, licensePlate, customerPrimaryKey);


                clearCustomerDetailValues();
                clearCustomerVehcileDetails();


                showNewCustomerPanel(pnlCustomerOptions);
                pnlCustomerOptions.BringToFront();
            }
            else
            {
                //Display appropiate message to the user
                MessageBox.Show($"Please fill out all fields before continuing.");
            }
        }

        private void btnCustomerVehicleInfoBack_Click_1(object sender, EventArgs e)
        {

            pnlCustomerDetails.BringToFront();

            showNewCustomerPanel(pnlCustomerDetails);
        }

        private void dgvViewAllCustomers_SelectionChanged(object sender, EventArgs e)
        {
            populateViewAllCustomerTextBoxes();
        }



        private void dgvDeleteCustomer_SelectionChanged(object sender, EventArgs e)
        {
            populateDeleteCustomerTextBoxes();
        }



        private void dgvViewAllVehicles_SelectionChanged(object sender, EventArgs e)
        {
            populateViewAllVehiclesTextBoxes();
        }

        private void btnDeleteCustomerClearFilter_Click(object sender, EventArgs e)
        {
            resetDeleteCustomerFilter();
        }

        private void btnPurchaseClearFilter_Click(object sender, EventArgs e)
        {
            txtProductNumberFilter.Clear();
            txtPurchaseProductName.Clear();
        }

        private void txtPurchaseProductName_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT Product_Number , Product_Name, Available_Quantity, Unit_Price FROM Inventory WHERE UPPER(Product_Name) LIKE '%{txtPurchaseProductName.Text.ToUpper()}%' AND UPPER(Product_Number) LIKE '%{txtProductNumberFilter.Text.ToUpper()}%'", dgvPurchaseGridView);
        }

        private void pnlMakePurcahes_Paint(object sender, PaintEventArgs e)
        {

        }

        public bool checkBookedService()
        {
            try
            {
                //Assign new connection
                conn = new SqlConnection(connectionString);

                // Open connection to the DB
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }


                DateTime selectedDate = dpBookingService.Value.Date;


                DateTime selectedTime = dpBookServiceTime.Value;


                DateTime combinedDateTime = selectedDate.Add(selectedTime.TimeOfDay);



                //Select all records in the table
                string sql = $"SELECT * FROM [Transaction] WHERE Booked_Date BETWEEN DATEADD(MINUTE, -30, '{combinedDateTime:yyyy-MM-dd HH:mm:ss}') AND DATEADD(MINUTE, 30, '{combinedDateTime:yyyy-MM-dd HH:mm:ss}') AND Status <> 'Completed' AND Transaction_ID IN (SELECT Transaction_ID FROM Transaction_Item WHERE Transaction_Description = 'Service')";



                // Initialize new Sql command
                command = new SqlCommand(sql, conn);

                // Execute command
                dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {

                    return true;
                }

                // Close conenction to DB
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                // Display suitable error dialog
                MessageBox.Show("An error has occured " + ex.Message);

                // Close connection if open
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                return true;

            }

            return false;
        }

        public bool validateServiceBookingInput()
        {
            if (string.IsNullOrEmpty(txtServiceCustomersNum.Text) && (string.IsNullOrEmpty(txtServiceDiscountAmount.Text)))
            {
                MessageBox.Show("Please fill out all fields before continuing");
                return false;
            }
            else if (DateTime.Now >= (dpBookingService.Value).Add(dpBookServiceTime.Value.TimeOfDay))
            {
                MessageBox.Show("Please select a valid upcoming date");
                return false;
            }
            else if (checkBookedService())
            {
                MessageBox.Show("There already exists a service that is booked within half an hour of the selected time. Please select another time.");
                return false;
            }
            else
            {
                return true;
            }
        }



        private void btnServiceMakeService_Click(object sender, EventArgs e)
        {

            if (validateServiceBookingInput())
            {

                const decimal SERVICEAMOUNT = 2500m;
                int client_Id = -1;
                string returnedClientId = getValueInTable($"SELECT Client_ID FROM Client WHERE Contact_Number = '{txtServiceCustomersNum.Text}'", 0);

                if (string.IsNullOrEmpty(returnedClientId))
                {
                    DialogResult result = MessageBox.Show("The client could not be found. Do you want to register the client?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Information);


                    if (result == DialogResult.Yes)
                    {
                        changeHeading("Manage customers", "Select an applicable option");
                        tbcHomepage.SelectedTab = tbpAddCustomer;
                        btnDeleteCustomer.Enabled = false;
                        showNewCustomerPanel(pnlCustomerOptions);
                        selectLabel(lblAddCustomer);
                    }

                }
                else
                {
                    client_Id = int.Parse(returnedClientId);
                    decimal discountAmount;

                    if (decimal.TryParse(txtServiceDiscountAmount.Text, out discountAmount))
                    {
                        DateTime selectedDate = dpBookingService.Value.Date;


                        DateTime selectedTime = dpBookServiceTime.Value;


                        DateTime combinedDateTime = selectedDate.Add(selectedTime.TimeOfDay);
                        addTransactionEntry(client_Id, decimal.Parse(txtServiceDiscountAmount.Text), SERVICEAMOUNT - decimal.Parse(txtServiceDiscountAmount.Text), userId, combinedDateTime, standartServiceItems, standartServiceItemQuantity, "Service");
                        MessageBox.Show("Service has been successfuly booked.");

                        txtServiceCustomersNum.Clear();
                        txtServiceDiscountAmount.Clear();
                        dpBookServiceTime.Value = DateTime.Today.AddHours(8);
                        dpBookingService.Value = DateTime.Today;
                        executeDisplaySql("SELECT DISTINCT Transact.Booked_Date, Transact.Status, Cust.First_Name, Cust.Last_Name FROM [Transaction] Transact JOIN Client Cust ON Transact.Client_ID = Cust.Client_ID JOIN Transaction_Item TransactItem ON Transact.Transaction_ID = TransactItem.Transaction_ID WHERE TransactItem.Transaction_Description = 'Service' AND Transact.Status <> 'Completed'", dagvBookingOfServices);

                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid decimal value for the discount amount.");
                    }
                }
            }
        }

        public string getValueInTable(string sql, int val)
        {

            string output = "";
            try
            {
                // Open connection to the DB
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                // Initialize new Sql command
                command = new SqlCommand(sql, conn);
                dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    output = dataReader.GetValue(val).ToString();

                }

                // Close conenction to DB
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
            return output;
        }
        public string getValueInDVG(string sql, int val)
        {

            string output = "";
            try
            {
                // Open connection to the DB
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                // Initialize new Sql command
                command = new SqlCommand(sql, conn);
                dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    output = dataReader.GetValue(val).ToString();
                }

                // Close conenction to DB
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
            return output;
        }

        public void descreaseItemQuantity(int ineventoryItemId, int quanity)
        {
            try
            {
                //Open Connection
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }


                string sql = $"UPDATE Inventory SET Available_Quantity = Available_Quantity - { quanity } WHERE Id = {ineventoryItemId}";

                command = new SqlCommand(sql, conn);

                command.ExecuteNonQuery();

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

            }
            catch (SqlException sqlException)
            {

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                Console.WriteLine($"Error: {sqlException.Message}");
            }

        }

        public void increaseItemQuantity(int ineventoryItemId, int quanity)
        {
            try
            {
                //Open Connection
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }


                string sql = $"UPDATE Inventory SET Available_Quantity = Available_Quantity + { quanity } WHERE Id = {ineventoryItemId}";

                command = new SqlCommand(sql, conn);

                command.ExecuteNonQuery();

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

            }
            catch (SqlException sqlException)
            {

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                Console.WriteLine($"Error: {sqlException.Message}");
            }

        }

        public void addTransactionItemEntries(string transActionDescription, List<string> items, List<int> itemQuantity, int transactionId)
        {
            for (int i = 0; i < items.Count; i++)
            {
                try
                {
                    //MessageBox.Show();
                    string futureValue = getValueInTable($"SELECT Id FROM Inventory WHERE UPPER(Product_Name) = '{items[i].ToString().ToUpper()}'", 0);
                    int Inventory_ID = int.Parse(futureValue);
                    descreaseItemQuantity(Inventory_ID, int.Parse(itemQuantity[i].ToString()));


                    //Open Connection
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                    string sql = $"INSERT INTO Transaction_Item (Transaction_ID, Transaction_Description, Inventory_ID, Quantity) VALUES (@transactionID, @transactionDescription, @inventoryID, @quantity)";

                    command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue("@transactionID", transactionId);
                    command.Parameters.AddWithValue("@transactionDescription", transActionDescription);
                    command.Parameters.AddWithValue("@inventoryID", Inventory_ID);
                    command.Parameters.AddWithValue("@quantity", itemQuantity[i]);
                    command.ExecuteNonQuery();

                    //Close connection
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }

                }
                catch (SqlException sqlException)
                {

                    //Close connection
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }

                    Console.WriteLine($"Error: {sqlException.Message}");

                    //Show suitable error message
                    MessageBox.Show("Could not add Transaction!" + sqlException.Message);

                    break;
                }
            }
        }

        private void addTransactionEntry(int Client_ID, decimal Discount_Amount, decimal Total_time, int User_ID, DateTime? bookedDate, List<string> usedItems, List<int> quantityOfItemsUsed, string transActionDescription)
        {
            try
            {
                //Open Connection
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                //Initialize new command
                string sql = $"INSERT INTO [Transaction] (Client_ID, Payment_Date, Discount_Amount, Total_Amount, User_ID, Status) VALUES (@Client_ID, @Payment_Date, @Discount_Amount, @Total_Amount, @User_ID, @Status)  SELECT SCOPE_IDENTITY()";
                command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@Client_ID", Client_ID);
                command.Parameters.AddWithValue("@Payment_Date", DateTime.Now);
                command.Parameters.AddWithValue("@Discount_Amount", Discount_Amount);
                command.Parameters.AddWithValue("@Total_Amount", Total_time);
                command.Parameters.AddWithValue("@User_ID", User_ID);
                command.Parameters.AddWithValue("@Status", "In Progress");
                //

                command.ExecuteNonQuery();

                int insertedPrimaryKey = Convert.ToInt32(command.ExecuteScalar());

                addTransactionItemEntries(transActionDescription, usedItems, quantityOfItemsUsed, insertedPrimaryKey);

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

            }
            catch (SqlException sqlException)
            {
                //Show suitable error message
                MessageBox.Show("Could not add Transaction!" + sqlException.Message);

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                Console.WriteLine($"Error: {sqlException.Message}");
            }
        }
        private void addTransactionItem(int Transaction_ID, string Transaction_Description, int Inventory_ID, int Quantity)
        {
            try
            {
                //Open Connection
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                //Initialize new command
                string sql = $"INSERT INTO Transaction_Item (Transaction_ID, Transaction_Description, Inventory_ID, Quantity) VALUES (@transactId, @transactionDescription, @inventoryID, @quantity)";
                command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@transactId", Transaction_ID);
                command.Parameters.AddWithValue("@transactionDescription", Transaction_Description);
                command.Parameters.AddWithValue("@inventoryID", Inventory_ID);
                command.Parameters.AddWithValue("@quantity", Quantity);
                command.ExecuteNonQuery();



                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                MessageBox.Show("Succesfully scheduled made purchase.");
            }
            catch (SqlException sqlException)
            {
                //Show suitable error message
                MessageBox.Show("Could not make the purchase!");

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                Console.WriteLine($"Error: {sqlException.Message}");
            }
        }

        private void btnChangeSaleRefund_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if any row is selected
                if (dgvChangeSaleGridView.SelectedRows.Count > 0)
                {
                    DataGridViewRow selectedRow = dgvChangeSaleGridView.SelectedRows[0];

                    int transactionId = int.Parse(txtChangeSaleTransactionId.Text.Substring(1, 1));
                    int occurrences = int.Parse(getValueInTable($"SELECT COUNT(*) FROM Transaction_Item WHERE Transaction_ID = {transactionId}", 0));

                    for (int i = 0; i < occurrences; i++)
                    {
                        int inventory_Id = int.Parse(getValueInTable($"SELECT I.Id FROM Inventory AS I INNER JOIN Transaction_Item AS TI ON I.Id = TI.Inventory_ID WHERE TI.Transaction_ID = '{transactionId}'", 0));
                        int quantity = int.Parse(getValueInTable($"SELECT Quantity FROM Transaction_Item Where Inventory_ID = {inventory_Id} AND Transaction_ID = {transactionId}", 0));
                        increaseItemQuantity(inventory_Id, quantity);
                        deleteRecord($"DELETE FROM [Transaction_Item] WHERE Transaction_ID = {transactionId} AND Inventory_ID = {inventory_Id} ", false);
                    }

                    deleteRecord($"DELETE FROM [Transaction] WHERE Transaction_ID = {transactionId}", false);


                    MessageBox.Show("Refund has been completed successfully");

                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void txtServiceDiscountAmount_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '-') && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }


            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void txtRepairClientNum_Leave(object sender, EventArgs e)
        {
            string returnedClientId = getValueInTable($"SELECT Client_ID FROM Client WHERE Contact_Number = '{txtRepairClientNum.Text}'", 0);

            if (string.IsNullOrEmpty(returnedClientId) && pnlRepair.Visible == true)
            {
                DialogResult result = MessageBox.Show("The client could not be found. Do you want to register the client?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Information);


                if (result == DialogResult.Yes)
                {
                    changeHeading("Manage customers", "Select an applicable option");
                    tbcHomepage.SelectedTab = tbpAddCustomer;
                    btnDeleteCustomer.Enabled = false;
                    showNewCustomerPanel(pnlCustomerOptions);
                    selectLabel(lblAddCustomer);
                }
                else
                {
                    txtRepairClientNum.Focus();
                }

            }
            else
            {
                repairClientId = int.Parse(returnedClientId);
                displayRepairInfo();
            }

            repairItems.Clear();
            repairItemsQuantity.Clear();
        }


        public void displayRepairInfo()
        {
            string firstName, lastName;
            firstName = getValueInTable($"SELECT First_Name FROM Client WHERE Client_ID = {repairClientId}", 0);
            lastName = getValueInTable($"SELECT Last_Name FROM Client WHERE Client_ID = {repairClientId}", 0);


            lbxRepairParts.Items.Clear();
            lbxRepairParts.Items.Add("Repair summary for " + firstName + " " + lastName);

            lbxRepairParts.Items.Add("");

            if (repairItems != null && repairItems.Count != 0)
            {
                for (int i = 0; i < repairItems.Count; i++)
                {
                    string result = "x" + repairItemsQuantity[i] + " " + repairItems[i];
                    lbxRepairParts.Items.Add(result);
                }
            }


        }

        public void displayPurchaseInfo()
        {
            string firstName, lastName;
            firstName = getValueInTable($"SELECT First_Name FROM Client WHERE Client_ID = {purchaseClientId}", 0);
            lastName = getValueInTable($"SELECT Last_Name FROM Client WHERE Client_ID = {purchaseClientId}", 0);


            lbxPurchaseProduct.Items.Clear();
            lbxPurchaseProduct.Items.Add("Purchase summary for " + firstName + " " + lastName);

            lbxPurchaseProduct.Items.Add("");

            if (purchaseItems != null && purchaseItems.Count != 0)
            {
                for (int i = 0; i < purchaseItems.Count; i++)
                {
                    string result = "x" + purchaseItemsQuantity[i] + " " + purchaseItems[i] + "-R" + purchaseItemsPrice[i];
                    lbxPurchaseProduct.Items.Add(result);
                }
            }


        }
        public void addRepairItem()
        {
            // Check if any row is selected
            if (dgvRepairsParts.SelectedRows.Count > 0)
            {


                // Get the selected row
                DataGridViewRow selectedRow = dgvRepairsParts.SelectedRows[0];

                // Access the cell values from the selected row using column indexes
                string productName = selectedRow.Cells["Product_Name"].Value.ToString();
                int availableQuantity = int.Parse(selectedRow.Cells["Available_Quantity"].Value.ToString());
                string productNumber = selectedRow.Cells["Product_Number"].Value.ToString();
                if (availableQuantity == 0)
                {
                    MessageBox.Show(productName + " is out of stock. Please try again later.");
                }
                else if (repairItems != null && repairItems.Count != 0)
                {
                    bool added = false;
                    for (int i = 0; i < repairItems.Count; i++)
                    {
                        if (repairItems[i] == productName)
                        {

                            repairItemsQuantity[i]++;


                            int Inventory_ID = int.Parse(getValueInTable($"SELECT Id FROM Inventory WHERE UPPER(Product_Number) ='{productNumber}'", 0));
                            descreaseItemQuantity(Inventory_ID, 1);
                            executeDisplaySql("SELECT Product_Name, Product_Number, Available_Quantity, Unit_Price FROM Inventory", dgvRepairsParts);
                            added = true;
                        }
                    }

                    if (added == false)
                    {
                        repairItems.Add(productName);
                        repairItemsQuantity.Add(1);

                        int Inventory_ID = int.Parse(getValueInTable($"SELECT Id FROM Inventory WHERE UPPER(Product_Number) ='{productNumber}'", 0));
                        descreaseItemQuantity(Inventory_ID, 1);
                        executeDisplaySql("SELECT Product_Name, Product_Number, Available_Quantity, Unit_Price FROM Inventory", dgvRepairsParts);
                    }
                }
                else
                {
                    repairItems.Add(productName);
                    repairItemsQuantity.Add(1);

                    int Inventory_ID = int.Parse(getValueInTable($"SELECT Id FROM Inventory WHERE UPPER(Product_Number) ='{productNumber}'", 0));
                    descreaseItemQuantity(Inventory_ID, 1);
                    executeDisplaySql("SELECT Product_Name, Product_Number, Available_Quantity, Unit_Price FROM Inventory", dgvRepairsParts);
                }



                displayRepairInfo();
            }

        }

        public void addPurchaseItem()
        {
            // Check if any row is selected
            if (dgvPurchaseGridView.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dgvPurchaseGridView.SelectedRows[0];

                // Access the cell values from the selected row using column indexes
                string productName = selectedRow.Cells["Product_Name"].Value.ToString();
                int availableQuantity = int.Parse(selectedRow.Cells["Available_Quantity"].Value.ToString());
                string productNumber = selectedRow.Cells["Product_Number"].Value.ToString();
                decimal unitPrice = decimal.Parse(selectedRow.Cells["Unit_Price"].Value.ToString());
                if (availableQuantity == 0)
                {
                    MessageBox.Show(productName + " is out of stock. Please try again later.");
                }
                else if (purchaseItems != null && purchaseItems.Count != 0)
                {
                    bool added = false;
                    for (int i = 0; i < purchaseItems.Count; i++)
                    {
                        if (purchaseItems[i] == productName)
                        {

                            purchaseItemsQuantity[i]++;
                            purchaseItemsPrice[i] += unitPrice;



                            int Inventory_ID = int.Parse(getValueInTable($"SELECT Id FROM Inventory WHERE UPPER(Product_Number) ='{productNumber}'", 0));
                            descreaseItemQuantity(Inventory_ID, 1);
                            executeDisplaySql("SELECT Product_Name, Product_Number, Available_Quantity, Unit_Price FROM Inventory", dgvPurchaseGridView);
                            added = true;
                        }
                    }

                    if (added == false)
                    {
                        purchaseItems.Add(productName);
                        purchaseItemsQuantity.Add(1);
                        purchaseItemsPrice.Add(unitPrice);

                        int Inventory_ID = int.Parse(getValueInTable($"SELECT Id FROM Inventory WHERE UPPER(Product_Number) ='{productNumber}'", 0));
                        descreaseItemQuantity(Inventory_ID, 1);
                        executeDisplaySql("SELECT Product_Name, Product_Number, Available_Quantity, Unit_Price FROM Inventory", dgvPurchaseGridView);
                    }
                }
                else
                {
                    purchaseItems.Add(productName);
                    purchaseItemsQuantity.Add(1);
                    purchaseItemsPrice.Add(unitPrice);

                    int Inventory_ID = int.Parse(getValueInTable($"SELECT Id FROM Inventory WHERE UPPER(Product_Number) ='{productNumber}'", 0));
                    descreaseItemQuantity(Inventory_ID, 1);
                    executeDisplaySql("SELECT Product_Name, Product_Number, Available_Quantity, Unit_Price FROM Inventory", dgvPurchaseGridView);
                }
                displayPurchaseInfo();
            }
        }

        public void removeRepaitProduct(string input)
        {
            int indexOfFirstSpace = input.IndexOf(' ');

            // Access the cell values from the selected row using column indexes
            string productName = input.Substring(indexOfFirstSpace + 1);



            if (repairItems != null && repairItems.Count != 0)
            {

                for (int i = 0; i < repairItems.Count; i++)
                {
                    if (repairItems[i] == productName)
                    {

                        repairItemsQuantity[i]--;

                        if (repairItemsQuantity[i] == 0)
                        {
                            repairItemsQuantity.RemoveAt(i);
                            repairItems.RemoveAt(i);
                        }
                    }
                }
            }

            displayRepairInfo();

        }

        public void removeRepairProduct(string input)
        {
            int indexOfSpace = input.IndexOf(' ');
            int indexOfDash = input.IndexOf('-');

            // Access the cell values from the selected row using column indexes
            string productName = input.Substring(indexOfSpace + 1, (indexOfDash - 1 - indexOfSpace));



            if (purchaseItems != null && purchaseItems.Count != 0)
            {

                for (int i = 0; i < purchaseItems.Count; i++)
                {
                    if (purchaseItems[i] == productName)
                    {

                        purchaseItemsQuantity[i]--;
                        purchaseItemsPrice[i] -= purchaseItemsPrice[i];

                        if (purchaseItemsQuantity[i] == 0)
                        {
                            purchaseItemsQuantity.RemoveAt(i);
                            purchaseItems.RemoveAt(i);
                            purchaseItemsPrice.RemoveAt(i);
                        }
                    }

                }

            }
            displayPurchaseInfo();
        }

        private void dgvRepairsParts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (string.IsNullOrEmpty(txtRepairClientNum.Text))
            {
                MessageBox.Show("Please fill in client details first");
            }
            else
            {
                addRepairItem();
            }
        }

        private void btnRepairRemoveFromList_Click(object sender, EventArgs e)
        {
            if (lbxRepairParts.SelectedIndex == -1 || lbxRepairParts.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a product to remove from the list");
            }
            else
            {
                removeRepaitProduct(lbxRepairParts.SelectedItem?.ToString());
            }
        }

        private void btnRepairScheduleRepair_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtRepairClientNum.Text) || String.IsNullOrEmpty(txtRepairDescription.Text))
            {
                MessageBox.Show("Please fill out all fields before continuing");
            }
            else if (lbxRepairParts.Items.Count < 3)
            {
                MessageBox.Show("Please add at least one item before continuing");
            }
            else
            {
                const decimal REPAIRAMOUNT = 2500m;
                int client_Id = -1;
                string returnedClientId = getValueInTable($"SELECT Client_ID FROM Client WHERE Contact_Number = '{txtRepairClientNum.Text}'", 0);

                client_Id = int.Parse(returnedClientId);

                addTransactionEntry(client_Id, 0, REPAIRAMOUNT, userId, null, repairItems, repairItemsQuantity, "Repair - " + txtRepairDescription.Text);
                MessageBox.Show("The repair has been successfully booked.");
                txtRepairDescription.Clear();
                txtRepairClientNum.Clear();
                returnToSales();
            }
        }

        private void txtPurchaseClientContactNum_Leave(object sender, EventArgs e)
        {
            if (pnlMakePurcahes.Visible == true)
            {

                string returnedClientId = getValueInTable($"SELECT Client_ID FROM Client WHERE Contact_Number = '{txtPurchaseClientContactNum.Text}'", 0);

                if (string.IsNullOrEmpty(returnedClientId))
                {
                    DialogResult result = MessageBox.Show("The client could not be found. Do you want to register the client?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Information);


                    if (result == DialogResult.Yes)
                    {
                        changeHeading("Manage customers", "Select an applicable option");
                        tbcHomepage.SelectedTab = tbpAddCustomer;
                        btnDeleteCustomer.Enabled = false;
                        showNewCustomerPanel(pnlCustomerOptions);
                        selectLabel(lblAddCustomer);
                    }
                    else
                    {
                        txtPurchaseClientContactNum.Focus();
                    }
                }
                else
                {
                    purchaseClientId = int.Parse(returnedClientId);
                    displayPurchaseInfo();
                }

                purchaseItems.Clear();
                purchaseItemsQuantity.Clear();
                purchaseItemsPrice.Clear();
            }

        }

        private void txtProductNumberFilter_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT Product_Number , Product_Name, Available_Quantity, Unit_Price FROM Inventory WHERE UPPER(Product_Name) LIKE '%{txtPurchaseProductName.Text.ToUpper()}%' AND UPPER(Product_Number) LIKE '%{txtProductNumberFilter.Text.ToUpper()}%'", dgvPurchaseGridView);
        }

        private void dgvPurchaseGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPurchaseClientContactNum.Text))
            {
                MessageBox.Show("Please fill in client details first");
            }
            else
            {
                addPurchaseItem();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (lbxPurchaseProduct.SelectedIndex == -1 || lbxPurchaseProduct.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a product to remove from the list");
            }
            else
            {
                removeRepairProduct(lbxPurchaseProduct.SelectedItem?.ToString());
            }
        }

        private void txtChangeSaleTransactionId_TextChanged(object sender, EventArgs e)
        {
            if (txtChangeSaleTransactionId.Text.Length == 3)
            {
                string firstName = txtChangeSaleTransactionId.Text.Substring(0, 1);
                string lastName = txtChangeSaleTransactionId.Text.Substring(2, 1);
                string transactId = txtChangeSaleTransactionId.Text.Substring(1, 1);
                filterRecords($"SELECT C.First_Name, C.Last_Name, T.Payment_Date, T.Total_Amount, TI.Transaction_Description FROM Client AS C INNER JOIN [Transaction] AS T ON C.Client_ID = T.Client_ID INNER JOIN Transaction_Item AS TI ON T.Transaction_ID = TI.Transaction_ID WHERE LEFT(C.First_Name, 1) = '{firstName}' AND LEFT(C.Last_Name, 1) = '{lastName}' AND T.Transaction_ID = '{transactId}'", dgvChangeSaleGridView);
            }
        }

        private void dgvChangeSaleGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if any row is selected
            if (dgvChangeSaleGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dgvChangeSaleGridView.SelectedRows[0];

                if (txtChangeSaleTransactionId.Text.Length != 3)
                {
                    MessageBox.Show("Fill out the correct transaction number before continuing");
                }
                else if (String.IsNullOrEmpty(txtRefundContactNumber.Text) || txtRefundContactNumber.Text.Length != 10)
                {
                    MessageBox.Show("Invalid customer details");
                }
                else
                {
                    string firstNameCheck = getValueInTable($"SELECT First_Name From [Client] WHERE Contact_Number = '{txtRefundContactNumber.Text}'", 0);
                    string lastNameCheck = getValueInTable($"SELECT Last_Name From [Client] WHERE Contact_Number = '{txtRefundContactNumber.Text}'", 0);

                    string selectedFirstName = selectedRow.Cells["First_Name"].Value.ToString().Substring(0, 1);
                    string selectedLastName = selectedRow.Cells["Last_Name"].Value.ToString().Substring(0, 1);
                    string selectedPaymentDate = selectedRow.Cells["Payment_Date"].Value.ToString();
                    string selectedTransactionKey = getValueInTable($"SELECT Transaction_ID FROM [Transaction] WHERE Payment_Date = '{selectedPaymentDate}'", 0);

                    if (selectedFirstName + selectedTransactionKey + selectedLastName == txtChangeSaleTransactionId.Text && !string.IsNullOrEmpty(firstNameCheck) && !string.IsNullOrEmpty(lastNameCheck) && firstNameCheck.Substring(0, 1) == selectedFirstName && lastNameCheck.Substring(0, 1) == selectedLastName)
                    {
                        filterRecords($"SELECT I.Product_Name, TI.Quantity FROM Inventory AS I INNER JOIN Transaction_Item AS TI ON I.Id = TI.Inventory_ID WHERE TI.Id = '{selectedTransactionKey}'", dgvTransactionItemsRefund);
                    }
                    else
                    {
                        MessageBox.Show("Transaction number or Contact Number does not match selected transaction");
                    }
                }
            }
        }

        private void txtViewTransactionClientNum_TextChanged(object sender, EventArgs e)
        {
            if (txtViewTransactionClientNum.Text.Length == 3)
            {
                string firstName = txtViewTransactionClientNum.Text.Substring(0, 1);
                string lastName = txtViewTransactionClientNum.Text.Substring(2, 1);
                string transactId = txtViewTransactionClientNum.Text.Substring(1, 1);
                filterRecords($"SELECT C.First_Name, C.Last_Name, T.Payment_Date, T.Total_Amount, TI.Transaction_Description FROM Client AS C INNER JOIN [Transaction] AS T ON C.Client_ID = T.Client_ID INNER JOIN Transaction_Item AS TI ON T.Transaction_ID = TI.Transaction_ID WHERE LEFT(C.First_Name, 1) = '{firstName}' AND LEFT(C.Last_Name, 1) = '{lastName}' AND T.Transaction_ID = '{transactId}'", dgvViewTransactionGridView);
            }
            else if (txtViewTransactionClientNum.Text.Length == 0)
            {
                filterRecords($"SELECT C.First_Name, C.Last_Name, T.Payment_Date, T.Total_Amount, TI.Transaction_Description FROM Client AS C INNER JOIN [Transaction] AS T ON C.Client_ID = T.Client_ID INNER JOIN Transaction_Item AS TI ON T.Transaction_ID = TI.Transaction_ID WHERE LEFT(C.First_Name, 1) LIKE '%%' AND LEFT(C.Last_Name, 1) LIKE '%%'", dgvViewTransactionGridView);
            }
        }

        public void buildReceipt(string saleDate, string mechName, string customerFullName, string customerEmail, string receiptNumber, List<string> items, string totalPrice)
        {
            lbViewTransactionReceipt.Items.Add("----------------------------------------------------------------------------------------------");
            lbViewTransactionReceipt.Items.Add("                          RECEIPT                         ");
            lbViewTransactionReceipt.Items.Add("----------------------------------------------------------------------------------------------");
            lbViewTransactionReceipt.Items.Add("Store: FIX IT WORKSHOP                                    ");
            lbViewTransactionReceipt.Items.Add("----------------------------------------------------------------------------------------------");
            lbViewTransactionReceipt.Items.Add($"Date: {saleDate}                                         ");
            lbViewTransactionReceipt.Items.Add($"Sales assistant: {mechName}                              ");
            lbViewTransactionReceipt.Items.Add($"Customer Name & Surname: {customerFullName}              ");
            lbViewTransactionReceipt.Items.Add($"Customer Email: {customerEmail}                          ");
            lbViewTransactionReceipt.Items.Add("----------------------------------------------------------------------------------------------");
            foreach (var item in items)
            {
                lbViewTransactionReceipt.Items.Add($"{item}                                         ");

            }
            lbViewTransactionReceipt.Items.Add("----------------------------------------------------------------------------------------------");
            lbViewTransactionReceipt.Items.Add($"Total Price: R{totalPrice}                                ");
            lbViewTransactionReceipt.Items.Add("----------------------------------------------------------------------------------------------");
            lbViewTransactionReceipt.Items.Add($"Receipt Number: {receiptNumber}                          ");
            lbViewTransactionReceipt.Items.Add("----------------------------------------------------------------------------------------------");
        }

        public List<string> buildReceiptItems(string transactId)
        {
            List<string> items = new List<string>();
            List<Int32> inventoryIds = new List<Int32>();
            try
            {

                try
                {
                    // Open connection to the DB
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }


                    // Initialize new Sql command
                    command = new SqlCommand($"SELECT Inventory_ID,Quantity FROM Transaction_Item WHERE Transaction_ID = {transactId}", conn);
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        inventoryIds.Add(dataReader.GetInt32(0));
                        items.Add("x" + dataReader.GetValue(0).ToString() + " ");
                    }

                    // Close conenction to DB
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return items;
                }

                int counter = 0;
                foreach (var id in inventoryIds)
                {
                    // Open connection to the DB
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }


                    // Initialize new Sql command
                    command = new SqlCommand($"SELECT Product_Name, Unit_Price FROM Inventory WHERE Id = {id}", conn);
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {

                        items[counter] = items[counter] + dataReader.GetValue(0).ToString() + " " + dataReader.GetValue(1).ToString();

                    }

                    // Close conenction to DB
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                    counter++;

                }
                return items;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return items;
            }
        }

        private void dgvViewTransactionGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvViewTransactionGridView.SelectedRows.Count > 0)
            {

                lbViewTransactionReceipt.Items.Clear();

                DataGridViewRow selectedRow = dgvViewTransactionGridView.SelectedRows[0];

                string paymentDate = selectedRow.Cells["Payment_Date"].Value.ToString();

                string transactId = getValueInTable($"SELECT Transaction_ID FROM [Transaction] WHERE Payment_Date = '{paymentDate}'", 0);
                string totalPrice = getValueInTable($"SELECT Total_Amount FROM [Transaction] WHERE Transaction_ID = {transactId}", 0);


                string mechId = getValueInTable($"SELECT User_ID FROM [Transaction] WHERE Transaction_ID = {transactId}", 0);

                string mechName = (getValueInTable($"SELECT First_Name FROM [User] WHERE User_ID = {mechId}", 0)) + " " + (getValueInTable($"SELECT Last_Name FROM [User] WHERE User_ID = {mechId}", 0));
                string custId = getValueInTable($"SELECT Client_ID FROM [Transaction] WHERE Transaction_ID = {transactId}", 0);
                string custLastName = getValueInTable($"SELECT Last_Name FROM [Client] WHERE Client_ID = {custId}", 0);
                string custFirstName = getValueInTable($"SELECT First_Name FROM [Client] WHERE Client_ID = {custId}", 0);
                string custFullName = custFirstName + " " + custLastName;
                string custEmail = getValueInTable($"SELECT Email FROM [Client] WHERE Client_ID = {custId}", 0);
                string modifiedEmail = replaceQuarterWithAsterisks(custEmail);
                string receiptNumber = custFirstName.Substring(0, 1) + transactId + custLastName.Substring(0, 1);

                buildReceipt(paymentDate, mechName, custFullName, modifiedEmail, receiptNumber, buildReceiptItems(transactId), totalPrice);
            }
        }

        static string replaceQuarterWithAsterisks(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            int length = input.Length;
            int quarterLength = length / 4;

            if (quarterLength == 0)
            {
                return input;
            }

            int startIndex = length / 2 - quarterLength / 2;

            string asterisks = new string('*', quarterLength);

            string result = input.Substring(0, startIndex) + asterisks + input.Substring(startIndex + quarterLength);

            return result;
        }



        private void btnViewTransactionPrintSlip_Click(object sender, EventArgs e)
        {
            if (lbViewTransactionReceipt.Items.Count > 0)
            {
                DataGridViewRow selectedRow = dgvViewTransactionGridView.SelectedRows[0];

                string paymentDate = selectedRow.Cells["Payment_Date"].Value.ToString();
                string transactId = getValueInTable($"SELECT Transaction_ID FROM [Transaction] WHERE Payment_Date = '{paymentDate}'", 0);
                string custId = getValueInTable($"SELECT Client_ID FROM [Transaction] WHERE Transaction_ID = {transactId}", 0);
                string custEmail = getValueInTable($"SELECT Email FROM [Client] WHERE Client_ID = {custId}", 0);


                string body = "";
                foreach (var item in lbViewTransactionReceipt.Items)
                {
                    body += item.ToString() + "\n";
                }

                sendEmail(custEmail, body);

            }
            else
            {
                MessageBox.Show("Please select a record before continuing");
            }

        }

        private void btnOrderStockPlaceOrder_Click(object sender, EventArgs e)
        {
            int updateAmount = int.Parse(txtOrderStockOrderAoumnt.Text);
            int currentAmount = int.Parse(getValueInTable($"SELECT Available_Quantity FROM Inventory WHERE Product_Name = '{txtOrderStockProductName.Text}'", 0));
            int newAmount = currentAmount + updateAmount;
            updateRecord($"UPDATE Inventory SET Available_Quantity = {newAmount} WHERE UPPER(Product_Name) = '{txtOrderStockProductName.Text}'");
        }

        private void txtOrderStockProductName_TextChanged(object sender, EventArgs e)
        {
            filterRecords($"SELECT Product_Number , Product_Name FROM Inventory WHERE UPPER(Product_Name) LIKE '%{txtOrderStockProductName.Text.ToUpper()}%'", dgvOrderStock);
        }

        private void dgvViewTransactionGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvOrderStock_CellClick(object sender, DataGridViewCellEventArgs e)
        {


        }

        private void btnOrderStockClearFilter_Click(object sender, EventArgs e)
        {
            txtOrderStockProductName.Clear();
            txtOrderStockOrderAoumnt.Clear();
        }

        private void btnOrderStockAutoOrder_Click(object sender, EventArgs e)
        {
            //while()
            //if()
        }

        private void showStockPanel(Panel selectedPanel)
        {

            pnlStockCheck.Visible = false;

            pnlStockCheck.Visible = false;

            selectedPanel.Visible = true;
        }

        private void pnlStockCheckAll_Paint(object sender, PaintEventArgs e)
        {

        }



        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void dgvOrderStock_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOrderStock.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dgvOrderStock.SelectedRows[0];

                string selectedProductName = selectedRow.Cells["Product_Name"].Value.ToString();
                txtOrderStockProductName.Text = selectedProductName;
            }
        }

        private void btnUpdateSupp_Click(object sender, EventArgs e)
        {
            if (!validateEmail(tbEmailSupp))
            {
                MessageBox.Show("Please enter a valid email");
            }
            else if (tbCNumberSupp.Text.Length != 10)
            {
                MessageBox.Show("Please enter a valid contact number");
            }
            else
            {
                try
                {

                    updateRecord($"UPDATE Supplier SET Name = '{dgvSupp[0, dgvSupp.CurrentRow.Index].Value.ToString()}', Contact_Number = '{tbCNumberSupp.Text}', Email = '{tbEmailSupp.Text}' WHERE Name = '{dgvSupp[0, dgvSupp.CurrentRow.Index].Value.ToString()}'");

                    tbEmailSupp.Clear();
                    tbNameSupp.Clear();
                    tbCNumberSupp.Clear();

                    // Close conenction to DB
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    dataAdapter = new SqlDataAdapter();
                    dataSet = new DataSet();
                    string sql = "SELECT Name,Contact_Number,Email FROM Supplier";
                    command = new SqlCommand(sql, conn);
                    dataAdapter.SelectCommand = command;
                    dataAdapter.Fill(dataSet, "Supplier");
                    dgvSupp.DataSource = dataSet;
                    dgvSupp.DataMember = "Supplier";

                    // Close conenction to DB
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
                catch (SqlException sqlex)
                {
                    MessageBox.Show(sqlex.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void btnClearProductFilter_Click(object sender, EventArgs e)
        {
            tbProductNumber.Clear();
            tbProductName.Clear();
            sedAdd.Value = 0;
            cbxSupplierStock.SelectedIndex = -1;
            tbPriceProductStock.Clear();
        }

        private void tbProductNumber_TextChanged(object sender, EventArgs e)//Zohan
        {
            //try
            //{
            //    if (conn.State != ConnectionState.Open)
            //    {
            //        conn.Open();
            //    }
            //    dataAdapter = new SqlDataAdapter();
            //    dataSet = new DataSet();
            //    string sql =
            //         $@"
            //    SELECT 
            //        Inventory.Product_Number,
            //        Inventory.Product_Name,
            //        Inventory.Available_Quantity,
            //        Inventory.Unit_Price,
            //        Supplier.Name AS Supplier_Name
            //    FROM 
            //        Inventory
            //    INNER JOIN 
            //        Supplier ON Inventory.Supplier_ID = Supplier.Supplier_ID
            //    WHERE Inventory.Product_Number LIKE '%{tbProductNumber.Text}%'";
            //    Console.WriteLine("Generated SQL Query: " + sql);
            //    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
            //    DataTable dataTable2 = new DataTable(sql);
            //    command = new SqlCommand(sql, conn);

            //    dataAdapter.SelectCommand = command;
            //    dataAdapter.Fill(dataTable2);
            //    dgvProductStock.DataSource = dataTable2;
            //    dgvProductStock.Refresh();
            //    //dgvSupp.DataMember = "Supplier";
            //    if (conn.State == ConnectionState.Open)
            //    {
            //        conn.Close();
            //    }
            //}
            //catch (SqlException sqlEx)
            //{

            //    MessageBox.Show(sqlEx.ToString());
            //}
            //catch (Exception Ex)
            //{

            //    MessageBox.Show(Ex.ToString());
            //}
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbProductNumber.Text) || (string.IsNullOrEmpty(tbProductName.Text)) || sedAdd.Value == 0 || cbxSupplierStock.SelectedIndex == -1 || string.IsNullOrEmpty(tbPriceProductStock.Text))
            {
                MessageBox.Show("Please fill out all fields before continuing");
            }
            else
            {
                try
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    dataSet = new DataSet();
                    string sql = $"INSERT INTO Inventory(Product_Number,Product_Name,Available_Quantity,Unit_Price,Supplier_ID) VALUES (@Value1,@Value2,@Value3,@Value4,@Value5)";
                    SqlCommand cmd0 = new SqlCommand(sql, conn);
                    cmd0.Parameters.AddWithValue("@Value1", tbProductNumber.Text);
                    cmd0.Parameters.AddWithValue("@Value2", tbProductName.Text);
                    cmd0.Parameters.AddWithValue("@Value3", sedAdd.Value);
                    cmd0.Parameters.AddWithValue("@Value4", decimal.Parse(tbPriceProductStock.Text));

                    string CommandStock = $"SELECT Supplier_ID FROM Supplier WHERE Name = '{cbxSupplierStock.SelectedItem.ToString()}'";
                    SqlCommand cmd = new SqlCommand(CommandStock, conn);
                    dataReader = cmd.ExecuteReader();
                    int suppID = 0;
                    while (dataReader.Read())
                    {
                        suppID = (int)dataReader.GetValue(0);

                    }
                    cmd0.Parameters.AddWithValue("@Value5", suppID);
                    dataReader.Close();
                    SqlDataAdapter adap = new SqlDataAdapter();

                    adap.InsertCommand = cmd0;
                    adap.InsertCommand.ExecuteNonQuery();
                    // conn.Close();
                    tbProductNumber.Clear();
                    tbProductName.Clear();
                    cbxSupplierStock.SelectedIndex = -1;
                    sedAdd.Value = 0;
                    tbPriceProductStock.Clear();
                    // conn.Open();
                    dgvProductStock.Refresh();
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }

                    MessageBox.Show("Product has been successfully added!");

                }
                catch (SqlException sqlEx)
                {

                    MessageBox.Show(sqlEx.ToString());
                }
                catch (Exception Ex)
                {

                    MessageBox.Show(Ex.ToString());
                }
            }
        }

        private void tbProductName_TextChanged(object sender, EventArgs e)
        {

        }

        private void dgvProductStock_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }





        private void btnDeleteProductStock_Click(object sender, EventArgs e)
        {



            if (string.IsNullOrEmpty(tbProductNumber.Text))
            {
                MessageBox.Show("Please select a record before continuing");
            }
            else
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete this record? This cannout be undone.", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);


                if (result == DialogResult.Yes)
                {
                    deleteRecord($"DELETE FROM Inventory Where Product_Number ='{tbProductNumber.Text}'", true);
                }

                tbProductNumber.Clear();
                tbProductName.Clear();
                sedAdd.Value = 0;
                cbxSupplierStock.SelectedIndex = -1;
                tbPriceProductStock.Clear();

                executeDisplaySql(@"SELECT Inventory.Product_Number, Inventory.Product_Name, Inventory.Available_Quantity, Inventory.Unit_Price, Supplier.Name AS Supplier_Name FROM Inventory INNER JOIN Supplier ON Inventory.Supplier_ID = Supplier.Supplier_ID", dgvProductStock);
            }

        }

        private void btnUpdateProductStock_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbProductNumber.Text))
            {
                MessageBox.Show("Please select a field before continuing");
            }
            else
            {
                string suppID = getValueInTable($"SELECT Supplier_ID FROM Supplier WHERE Name = '{cbxSupplierStock.SelectedItem.ToString()}'", 0);
                updateRecord($"UPDATE Inventory SET  Product_Number = '{tbProductNumber.Text}', Product_Name = '{tbProductName.Text}', Available_Quantity = { sedAdd.Text}, Unit_Price = { tbPriceProductStock.Text}, Supplier_ID = { suppID} WHERE Product_Number = '{ dgvProductStock.SelectedRows[0].Cells["Product_Number"].Value}'");
            }


        }

        private void btnClear_View_All_Users_panel_Click(object sender, EventArgs e)
        {

            txtFirst_Name_View_All_Users_panel.Clear();
            txtLast_Name_View_All_Users_panel.Clear();

            cBUserType_View_All_Users_panel.SelectedIndex = -1;

            executeDisplaySql("SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User]", dGVDisplay_Users_View_All_Users_panel);
            pnlView_All_Users_panel.BringToFront();
        }

        private void txtFirst_Name_View_All_Users_panel_TextChanged(object sender, EventArgs e)
        {
            //Live filter the database with the FirstName

            executeDisplaySql($"SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User] WHERE User_Role = '{cBUserType_View_All_Users_panel.Text}' AND UPPER(First_Name) LIKE '%{txtFirst_Name_View_All_Users_panel.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtLast_Name_View_All_Users_panel.Text.ToUpper()}%'", dGVDisplay_Users_View_All_Users_panel);
        }

        private void txtLast_Name_View_All_Users_panel_TextChanged(object sender, EventArgs e)
        {
            //Live filter the database with the LastName
            executeDisplaySql($"SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User] WHERE User_Role = '{cBUserType_View_All_Users_panel.Text}' AND UPPER(First_Name) LIKE '%{txtFirst_Name_View_All_Users_panel.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtLast_Name_View_All_Users_panel.Text.ToUpper()}%'", dGVDisplay_Users_View_All_Users_panel);
        }

        private void cBUserType_View_All_Users_panel_SelectedIndexChanged(object sender, EventArgs e)
        {
            executeDisplaySql($"SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User] WHERE User_Role = '{cBUserType_View_All_Users_panel.Text}' AND UPPER(First_Name) LIKE '%{txtFirst_Name_View_All_Users_panel.Text.ToUpper()}%' AND UPPER(Last_Name) LIKE '%{txtLast_Name_View_All_Users_panel.Text.ToUpper()}%'", dGVDisplay_Users_View_All_Users_panel);
        }

        private void btnCancel_View_All_Users_panel_Click_1(object sender, EventArgs e)
        {
            showNewUserPanel(pnlUsers);
        }

        private void btnClear_Update_User_Details_panel_Click(object sender, EventArgs e)
        {
            txtFirstName_Update_User_Details_panel.Clear();
            txtLastName_Update_User_Details_panel.Clear();

            txtContactNumber_Update_User_Details_panel.Clear();
            txtEmail_Update_User_Details_panel.Clear();
            cBUserRole_Update_User_Details_panel.SelectedIndex = -1;
            txtNewPassword_Update_User_Details_panel.Clear();
        }

        private string getUserPassword(string firstName, string lastName, string contactNumber, string email, string userRole)
        {

            string password = "";
            try
            {
                // Open connection to the DB
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                //Count all matching emails to check for duplicates
                String sql = $"SELECT Password FROM [User] WHERE UPPER(Email)  = '{email.ToUpper()}' AND UPPER(First_Name)  = '{firstName.ToUpper()}' AND UPPER(Last_Name)  = '{lastName.ToUpper()}' AND Contact_Number  = '{contactNumber}' AND UPPER(User_Role)  = '{userRole.ToUpper()}'";

                // Initialize new Sql command
                command = new SqlCommand(sql, conn);

                // Execute command
                dataReader = command.ExecuteReader();



                while (dataReader.Read())
                {
                    password = dataReader.GetValue(0).ToString();
                }
                // Close conenction to DB
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                // Display suitable error dialog
                MessageBox.Show("An error has occured " + ex.Message);

                // Close connection if open
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return password;
        }



        private void populateUpdateUserDetailsTextBoxes()
        {
            try
            {
                if (dGV_Update_User_Details_panel.SelectedRows.Count > 0)
                {
                    // Get the selected row
                    DataGridViewRow selectedRow = dGV_Update_User_Details_panel.SelectedRows[0];

                    if (!isAdmin && userId != int.Parse((getValueInTable($"SELECT User_ID FROM [USER] WHERE Contact_Number = '{selectedRow.Cells["Contact_Number"].Value.ToString()}' AND Email = '{selectedRow.Cells["Email"].Value.ToString()}'", 0))))
                    {
                        MessageBox.Show("You do not have permission to update this record", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    }
                    else
                    {
                        // Access the cell values from the selected row using column indexes
                        string firstName = selectedRow.Cells["First_Name"].Value.ToString();
                        string lastName = selectedRow.Cells["Last_Name"].Value.ToString();

                        string contactNumber = selectedRow.Cells["Contact_Number"].Value.ToString();
                        string email = selectedRow.Cells["Email"].Value.ToString();

                        string userRole = selectedRow.Cells["User_Role"].Value.ToString();
                        if (userRole.Trim() == "Admin")
                        {
                            cBUserRole_Update_User_Details_panel.SelectedIndex = 0;
                        }
                        else if (userRole.Trim() == "Mechanic")
                        {
                            cBUserRole_Update_User_Details_panel.SelectedIndex = 1;
                        }
                        else
                        {
                            cBUserRole_Update_User_Details_panel.SelectedIndex = -1;
                        }


                        txtFirstName_Update_User_Details_panel.Text = firstName;
                        txtLastName_Update_User_Details_panel.Text = lastName;

                        txtContactNumber_Update_User_Details_panel.Text = contactNumber;
                        txtEmail_Update_User_Details_panel.Text = email;

                        txtNewPassword_Update_User_Details_panel.Text = (getUserPassword(firstName, lastName, contactNumber, email, userRole));

                        btnUpdate_User_Details_Update_User_Details_panel.Enabled = true;

                    }

                }
                else
                {
                    btnUpdate_User_Details_Update_User_Details_panel.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

       

        private void dGV_Update_User_Details_panel_SelectionChanged(object sender, EventArgs e)
        {

            populateUpdateUserDetailsTextBoxes();
        }

        private void btnCancel_Update_User_Details_panel_Click_1(object sender, EventArgs e)
        {
            showNewUserPanel(pnlUsers);
        }

        private int getUserId(string firstName, string lastName, string contactNumber, string email)
        {
            {

                int selectedUserId = -1;
                try
                {
                    // Open connection to the DB
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }

                    //Count all matching emails to check for duplicates
                    String sql = $"SELECT User_ID FROM [User] WHERE UPPER(Email)  = '{email.ToUpper()}' AND UPPER(First_Name)  = '{firstName.ToUpper()}' AND UPPER(Last_Name)  = '{lastName.ToUpper()}' AND Contact_Number  = '{contactNumber}'";

                    // Initialize new Sql command
                    command = new SqlCommand(sql, conn);

                    // Execute command
                    dataReader = command.ExecuteReader();



                    while (dataReader.Read())
                    {
                        selectedUserId = dataReader.GetInt32(0);
                    }
                    // Close conenction to DB
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    // Display suitable error dialog
                    MessageBox.Show("An error has occured " + ex.Message);

                    // Close connection if open
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }

                return selectedUserId;
            }
        }

        private void btnUpdate_User_Details_Update_User_Details_panel_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFirstName_Update_User_Details_panel.Text) || string.IsNullOrEmpty(txtLastName_Update_User_Details_panel.Text) || string.IsNullOrEmpty(txtContactNumber_Update_User_Details_panel.Text) || string.IsNullOrEmpty(txtEmail_Update_User_Details_panel.Text) || (cBUserRole_Update_User_Details_panel.SelectedIndex == -1) || string.IsNullOrEmpty(txtNewPassword_Update_User_Details_panel.Text))
            {
                MessageBox.Show("Please fill out all fields and enter the appropriate values.");
            }
            else if (!validateEmail(txtEmail_Update_User_Details_panel))
            {
                MessageBox.Show("Please enter a valid email");
            }
            else if (txtContactNumber_Update_User_Details_panel.Text.Length != 10)
            {
                MessageBox.Show("Please enter a valid contact number");
            }
            else
            {
                // Call the function that updates the values in the table
                DataGridViewRow selectedRow = dGV_Update_User_Details_panel.SelectedRows[0];

                int selectedUserId = getUserId(selectedRow.Cells["First_Name"].Value.ToString(), selectedRow.Cells["Last_Name"].Value.ToString(), selectedRow.Cells["Contact_Number"].Value.ToString(), selectedRow.Cells["Email"].Value.ToString());
                updateRecord($"UPDATE [User] SET First_name = '{txtFirstName_Update_User_Details_panel.Text}', Last_Name = '{txtLastName_Update_User_Details_panel.Text}', Email = '{txtEmail_Update_User_Details_panel.Text}', Contact_Number = '{txtContactNumber_Update_User_Details_panel.Text}', User_Role = '{cBUserRole_Update_User_Details_panel.Text}', Password = '{(txtNewPassword_Update_User_Details_panel.Text)}' WHERE User_ID = ${selectedUserId}");
                executeDisplaySql("SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User]", dGV_Update_User_Details_panel);

                txtFirstName_Update_User_Details_panel.Clear();
                txtLastName_Update_User_Details_panel.Clear();

                txtContactNumber_Update_User_Details_panel.Clear();
                txtEmail_Update_User_Details_panel.Clear();
                cBUserRole_Update_User_Details_panel.SelectedIndex = -1;
                txtNewPassword_Update_User_Details_panel.Clear();
            }
        }

        private void txtLastName_pnlRemove_Users_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnClear_pnlRemove_Users_Textboxes_Click(object sender, EventArgs e)
        {
            txtFirstName_Remove_User_by_Username.Clear();
            txtLastName_pnlRemove_Users.Clear();
            txtEmail_Remove_Users_panel.Clear();
            txtContact_Number_pnlRemove_Users.Clear();
        }

        private void populateDeleteUserDetailsTextBoxes()
        {
            try
            {
                if (dGV_pnlRemove_Users_Display.SelectedRows.Count > 0)
                {
                    // Get the selected row
                    DataGridViewRow selectedRow = dGV_pnlRemove_Users_Display.SelectedRows[0];

                    // Access the cell values from the selected row using column indexes
                    string firstName = selectedRow.Cells["First_Name"].Value.ToString();
                    string lastName = selectedRow.Cells["Last_Name"].Value.ToString();

                    string contactNumber = selectedRow.Cells["Contact_Number"].Value.ToString();
                    string email = selectedRow.Cells["Email"].Value.ToString();


                    txtFirstName_Remove_User_by_Username.Text = firstName;
                    txtLastName_pnlRemove_Users.Text = lastName;

                    txtContact_Number_pnlRemove_Users.Text = contactNumber;
                    txtEmail_Remove_Users_panel.Text = email;

                    btnRemove_User_panel.Enabled = true;
                }
                else
                {
                    btnRemove_User_panel.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void dGV_pnlRemove_Users_Display_SelectionChanged(object sender, EventArgs e)
        {
            populateDeleteUserDetailsTextBoxes();
        }

        private void removeUser(int selectedUserId)
        {

            try
            {
                conn = new SqlConnection(connectionString);
                // Close connection if open
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                dataSet = new DataSet();
                string sql = $"DELETE FROM [User] Where User_ID = '{selectedUserId}'";

                command = new SqlCommand(sql, conn);
                SqlDataAdapter adap = new SqlDataAdapter();

                //Change InsertCommand to DeleteCommand
                adap.DeleteCommand = command;
                adap.DeleteCommand.ExecuteNonQuery();


                // Close connection if open
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }


            }
            catch (Exception ex)
            {
                // Display suitable error dialog
                MessageBox.Show("An error has occured " + ex.Message);

                // Close connection if open
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }


        }

        private void btnRemove_User_panel_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFirstName_Remove_User_by_Username.Text) || string.IsNullOrEmpty(txtLastName_pnlRemove_Users.Text) || string.IsNullOrEmpty(txtEmail_Remove_Users_panel.Text) || string.IsNullOrEmpty(txtContact_Number_pnlRemove_Users.Text))
            {
                MessageBox.Show("You have empty textboxes. \n Enter the appropriate values.");
            }
            else
            {

                DialogResult result = MessageBox.Show("Are you sure you want to delete this user?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    int selectedUser = getUserId(txtFirstName_Remove_User_by_Username.Text, txtLastName_pnlRemove_Users.Text, txtContact_Number_pnlRemove_Users.Text, txtEmail_Remove_Users_panel.Text);
                    removeUser(selectedUser);
                    executeDisplaySql("SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User]", dGV_pnlRemove_Users_Display);
                    txtFirstName_Remove_User_by_Username.Clear();
                    txtLastName_pnlRemove_Users.Clear();
                    txtEmail_Remove_Users_panel.Clear();
                    txtContact_Number_pnlRemove_Users.Clear();

                    MessageBox.Show($"User successfully deleted");
                }


            }
        }

        private void btnCancel_on_RemoveUser_panel_Click_1(object sender, EventArgs e)
        {
            showNewUserPanel(pnlUsers);
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            txtUsername_AddUsers_panel.Clear();
            txtFirstName_AddUsers_panel.Clear();
            txtLastName_AddUsers_panel.Clear();
            txtContactNumber_AddUsers_panel.Clear();
            txtEmail_AddUsers_panel.Clear();
            cBUserRole_AddUsers_panel.SelectedIndex = -1;
            txtPassword_AddUsers_panel.Clear();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            showNewUserPanel(pnlUsers);
        }

        private void add_New_User_AddUsers_panel(string username, string firstName, string lastName, string email, string contactNumber, string user_role, string password)
        {
            try
            {
                //Assign new connection
                conn = new SqlConnection(connectionString);

                //Open Connection
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                //Initialize new command
                string sql = $"INSERT INTO [User] (Username, First_Name, Last_Name, Email, Contact_Number, User_Role, Password) VALUES (@username ,@first_name, @last_name, @email, @contact_number, @user_role, @password)";
                command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@first_name", firstName);
                command.Parameters.AddWithValue("@last_name", lastName);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@contact_number", contactNumber);
                command.Parameters.AddWithValue("@user_role", user_role);
                command.Parameters.AddWithValue("@password", password);



                command.ExecuteNonQuery();

                //Display appropiate message to the user
                MessageBox.Show($"{firstName} {lastName} has been successfully registered.");

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }


            }
            catch (SqlException sqlException)
            {
                //Show suitable error message
                MessageBox.Show("Sign up failed.\nPlease try again later.");

                //Close connection
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                Console.WriteLine($"Error: {sqlException.Message}");
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername_AddUsers_panel.Text) || string.IsNullOrEmpty(txtFirstName_AddUsers_panel.Text) || string.IsNullOrEmpty(txtLastName_AddUsers_panel.Text) || string.IsNullOrEmpty(txtContactNumber_AddUsers_panel.Text) || string.IsNullOrEmpty(txtEmail_AddUsers_panel.Text) || (cBUserRole_AddUsers_panel.SelectedIndex == -1) || string.IsNullOrEmpty(txtPassword_AddUsers_panel.Text))
            {
                MessageBox.Show("You have empty textboxes. \n Enter the appropriate values.");
            }
            else
            {
                add_New_User_AddUsers_panel(txtUsername_AddUsers_panel.Text, txtFirstName_AddUsers_panel.Text, txtLastName_AddUsers_panel.Text, txtContactNumber_AddUsers_panel.Text, txtEmail_AddUsers_panel.Text, cBUserRole_AddUsers_panel.Text, txtPassword_AddUsers_panel.Text);

                executeDisplaySql("SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User]", dGV_Add_New_Users_panel);

                txtUsername_AddUsers_panel.Clear();
                txtFirstName_AddUsers_panel.Clear();
                txtLastName_AddUsers_panel.Clear();
                txtContactNumber_AddUsers_panel.Clear();
                txtEmail_AddUsers_panel.Clear();
                cBUserRole_AddUsers_panel.SelectedIndex = -1;
                txtPassword_AddUsers_panel.Clear();
            }
        }

        private void btn_Clear_AddUsers_panel_Click(object sender, EventArgs e)
        {
            txtUsername_AddUsers_panel.Clear();
            txtFirstName_AddUsers_panel.Clear();
            txtLastName_AddUsers_panel.Clear();
            txtContactNumber_AddUsers_panel.Clear();
            txtEmail_AddUsers_panel.Clear();
            cBUserRole_AddUsers_panel.SelectedIndex = -1;
            txtPassword_AddUsers_panel.Clear();
        }

        private void btn_Cancel_AddUsers_panel_Click_1(object sender, EventArgs e)
        {
            showNewUserPanel(pnlUsers);

        }

        private void btnAdd_New_User_AddUsers_panel_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername_AddUsers_panel.Text) || string.IsNullOrEmpty(txtFirstName_AddUsers_panel.Text) || string.IsNullOrEmpty(txtLastName_AddUsers_panel.Text) || string.IsNullOrEmpty(txtContactNumber_AddUsers_panel.Text) || string.IsNullOrEmpty(txtEmail_AddUsers_panel.Text) || (cBUserRole_AddUsers_panel.SelectedIndex == -1) || string.IsNullOrEmpty(txtPassword_AddUsers_panel.Text))
            {
                MessageBox.Show("You have empty textboxes. \n Enter the appropriate values.");
            }
            else if (!validateEmail(txtEmail_AddUsers_panel))
            {
                MessageBox.Show("Please enter a valid email");
            }
            else if (txtContactNumber_AddUsers_panel.Text.Length != 10)
            {
                MessageBox.Show("Please enter a valid contact number");
            }
            else
            {
                add_New_User_AddUsers_panel(txtUsername_AddUsers_panel.Text, txtFirstName_AddUsers_panel.Text, txtLastName_AddUsers_panel.Text, txtContactNumber_AddUsers_panel.Text, txtEmail_AddUsers_panel.Text, cBUserRole_AddUsers_panel.Text, (txtPassword_AddUsers_panel.Text));

                executeDisplaySql("SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User]", dGV_Add_New_Users_panel);

                txtUsername_AddUsers_panel.Clear();
                txtFirstName_AddUsers_panel.Clear();
                txtLastName_AddUsers_panel.Clear();
                txtContactNumber_AddUsers_panel.Clear();
                txtEmail_AddUsers_panel.Clear();
                cBUserRole_AddUsers_panel.SelectedIndex = -1;
                txtPassword_AddUsers_panel.Clear();
            }
        }

        private void btnView_All_Users_Click(object sender, EventArgs e)
        {
            showNewUserPanel(pnlView_All_Users_panel);
            executeDisplaySql("SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User]", dGVDisplay_Users_View_All_Users_panel);
            pnlView_All_Users_panel.BringToFront();
        }

        private void btnAdd_New_Users_Click(object sender, EventArgs e)
        {
            showNewUserPanel(pnlAdd_New_Users);
            executeDisplaySql("SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User]", dGV_Add_New_Users_panel);
            pnlAdd_New_Users.BringToFront();
        }

        private void btnUpdate_User_Details_Click(object sender, EventArgs e)
        {
            showNewUserPanel(pnlUpdate_User_Details);
            executeDisplaySql("SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User]", dGV_Update_User_Details_panel);
            btnUpdate_User_Details_Update_User_Details_panel.Enabled = false;
            pnlUpdate_User_Details.BringToFront();
        }

        private void btnRemove_Users_Click(object sender, EventArgs e)
        {

            showNewUserPanel(pnlRemove_Users);
            executeDisplaySql("SELECT First_Name, Last_Name, Email, Contact_Number, User_Role FROM [User]", dGV_pnlRemove_Users_Display);
            pnlRemove_Users.BringToFront();
            btnRemove_User_panel.Enabled = false;
        }

        public Dictionary<string, int> getTopProductsFromTable(int amount)
        {
            Dictionary<string, int> topProdcuts = new Dictionary<string, int>();
            try
            {
                // Open connection to the DB
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }



                // Initialize new Sql command
                command = new SqlCommand($" SELECT DISTINCT Inventory_ID, SUM(Quantity) AS TotalQuantity FROM Transaction_Item WHERE Transaction_Description = 'Sale' GROUP BY Inventory_ID ORDER BY TotalQuantity DESC", conn);

                // Execute command
                dataReader = command.ExecuteReader();


                int counter = 0;
                int[] inventoryIds = new int[amount];
                int[] quantityTotal = new int[amount];
                while (dataReader.Read() && counter < amount)
                {
                    inventoryIds[counter] = int.Parse(dataReader.GetValue(0).ToString());
                    quantityTotal[counter] = int.Parse(dataReader.GetValue(1).ToString());
                    counter++;
                }
                // Close conenction to DB
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                for (int i = 0; i < inventoryIds.Length; i++)
                {
                    string itemName = getValueInTable($"SELECT Product_Name FROM Inventory WHERE Id = {inventoryIds[i]}", 0);

                    if (!topProdcuts.ContainsKey(itemName))
                    {
                        topProdcuts.Add(itemName, quantityTotal[i]);
                    }


                }
            }
            catch (Exception ex)
            {
                // Display suitable error dialog
                MessageBox.Show("An error has occured " + ex.Message);

                // Close connection if open
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return topProdcuts;
        }

        public void populateChart(int amount)
        {
            Dictionary<string, int> topProducts = getTopProductsFromTable(amount);

            crtBestSellingProducts.Series["Inventory"].Points.Clear();
            foreach (var product in topProducts)
            {
                crtBestSellingProducts.Series["Inventory"].Points.AddXY(product.Key, product.Value);
            }
        }

        public double[] getFinData(string type)
        {
            double[] selectedRow = new double[150];
            try
            {
                // Open connection to the DB
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                int selectedIndex = 0;

                //switch (type)
                //{
                //    case ("Month"):
                //        {
                //            command = new SqlCommand($"SELECT MONTH(T.Payment_Date) AS Month FROM Transaction_Item TI JOIN [Transaction] T ON TI.Transaction_ID = T.Transaction_ID GROUP BY MONTH(T.Payment_Date) ORDER BY Month", conn);
                //            break;
                //        }

                //    case ("Service"):
                //        {
                //            command = new SqlCommand($"SELECT MONTH(T.Payment_Date) AS Month, SUM(CASE WHEN TI.Transaction_Description = 'Service' THEN T.Total_Amount - T.Discount_Amount ELSE 0 END) AS TotalServices FROM Transaction_Item TI JOIN [Transaction] T ON TI.Transaction_ID = T.Transaction_ID GROUP BY MONTH(T.Payment_Date) ORDER BY Month", conn);
                //            selectedIndex = 1;
                //            break;
                //        }

                //    case ("Sale"):
                //        {
                //            command = new SqlCommand($"SELECT MONTH(T.Payment_Date) AS Month, SUM(CASE WHEN TI.Transaction_Description = 'Sale' THEN T.Total_Amount - T.Discount_Amount ELSE 0 END) AS TotalSales FROM Transaction_Item TI JOIN [Transaction] T ON TI.Transaction_ID = T.Transaction_ID GROUP BY MONTH(T.Payment_Date) ORDER BY Month", conn);
                //            selectedIndex = 1;
                //            break;
                //        }

                //    case ("Repair"):
                //        {
                //            command = new SqlCommand($"SELECT MONTH(T.Payment_Date) AS Month, SUM(CASE WHEN TI.Transaction_Description = 'Repair' THEN T.Total_Amount - T.Discount_Amount ELSE 0 END) AS TotalRepairs FROM Transaction_Item TI JOIN [Transaction] T ON TI.Transaction_ID = T.Transaction_ID GROUP BY MONTH(T.Payment_Date) ORDER BY Month", conn);
                //            selectedIndex = 1;
                //            break;
                //        }

                //    case ("Replenishment"):
                //        {
                //            command = new SqlCommand($"SELECT MONTH(T.Payment_Date) AS Month, SUM(CASE WHEN TI.Transaction_Description = 'Replenishment' THEN T.Total_Amount - T.Discount_Amount ELSE 0 END) AS TotalReplenishments FROM Transaction_Item TI JOIN [Transaction] T ON TI.Transaction_ID = T.Transaction_ID GROUP BY MONTH(T.Payment_Date) ORDER BY Month", conn);
                //            selectedIndex = 1;
                //            break;
                //        }

                //    default:
                //        command = new SqlCommand($"SELECT MONTH(T.Payment_Date) AS Month, SUM(CASE WHEN TI.Transaction_Description = 'Sale' THEN T.Total_Amount - T.Discount_Amount ELSE 0 END) AS TotalSales, SUM(CASE WHEN TI.Transaction_Description = 'Service' THEN T.Total_Amount - T.Discount_Amount ELSE 0 END) AS TotalServices, SUM(CASE WHEN TI.Transaction_Description = 'Repair' THEN T.Total_Amount - T.Discount_Amount ELSE 0 END) AS TotalRepairs, SUM(CASE WHEN TI.Transaction_Description = 'Replenishment' THEN T.Total_Amount - T.Discount_Amount ELSE 0 END) AS TotalReplenishments FROM Transaction_Item TI JOIN [Transaction] T ON TI.Transaction_ID = T.Transaction_ID GROUP BY MONTH(T.Payment_Date) ORDER BY Month", conn);
                //        break;
                //}

                // Initialize new Sql command
                command = new SqlCommand($"SELECT SUM(CASE WHEN TI.Transaction_Description = 'Sale' THEN T.Total_Amount - T.Discount_Amount ELSE 0 END) AS TotalSales, SUM(CASE WHEN TI.Transaction_Description = 'Service' THEN T.Total_Amount - T.Discount_Amount ELSE 0 END) AS TotalServices, SUM(CASE WHEN TI.Transaction_Description = 'Repair' THEN T.Total_Amount - T.Discount_Amount ELSE 0 END) AS TotalRepairs, SUM(CASE WHEN TI.Transaction_Description = 'Replenishment' THEN T.Total_Amount - T.Discount_Amount ELSE 0 END) AS TotalReplenishments FROM Transaction_Item TI JOIN [Transaction] T ON TI.Transaction_ID = T.Transaction_ID GROUP BY MONTH(T.Payment_Date) ORDER BY Month", conn);

                // Execute command
                dataReader = command.ExecuteReader();


                int counter = 0;
                while (dataReader.Read())
                {
                    selectedRow[counter] = double.Parse(dataReader.GetValue(selectedIndex).ToString());
                    selectedRow[counter + 1] = double.Parse(dataReader.GetValue(selectedIndex).ToString());
                    selectedRow[counter + 2] = double.Parse(dataReader.GetValue(selectedIndex).ToString());
                    selectedRow[counter + 3] = double.Parse(dataReader.GetValue(selectedIndex).ToString());
                    counter++;
                }
                // Close conenction to DB
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                // Display suitable error dialog
                MessageBox.Show("An error has occured " + ex.Message);

                // Close connection if open
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return selectedRow;
        }


        public void switchBestSellingProductView()
        {
            crtBestSellingProducts.Visible = !crtBestSellingProducts.Visible;
            dgvBestSellingProductsSummary.Visible = !dgvBestSellingProductsSummary.Visible;

            if (crtBestSellingProducts.Visible)
            {
                btnSwitchBestSellingProductView.Text = "Text Based Representation";
            }
            else
            {
                btnSwitchBestSellingProductView.Text = "Chart Based Representation";
            }
        }

        public void populateDataGridView(int amount)
        {
            executeDisplaySql($"SELECT TOP {amount} I.Product_Name, SUM(CASE WHEN TI.Transaction_Description = 'Sale' THEN TI.Quantity ELSE 0 END) AS SaleQuantity, SUM(CASE WHEN TI.Transaction_Description = 'Service' THEN TI.Quantity ELSE 0 END) AS ServiceQuantity, SUM(CASE WHEN TI.Transaction_Description = 'Repair' THEN TI.Quantity ELSE 0 END) AS RepairQuantity, SUM(CASE WHEN TI.Transaction_Description = 'Replenishment' THEN TI.Quantity ELSE 0 END) AS ReplenishmentQuantity FROM Transaction_Item TI JOIN Inventory I ON TI.Inventory_ID = I.Id GROUP BY TI.Inventory_ID, I.Product_Name ORDER BY SaleQuantity DESC", dgvBestSellingProductsSummary);
        }

        private void btnTopPerformingProducts_Click_1(object sender, EventArgs e)
        {
            dgvBestSellingProductsSummary.Visible = true;
            crtBestSellingProducts.Visible = false;

            showReports(pnlReportsBestInventory);


            populateChart(5);
            populateDataGridView(5);
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            lblSelectBestPerformingProducts.Text = "Top 10 best selling products:";
            populateChart(10);
            populateDataGridView(10);
        }

        private void btnTop5Products_Click(object sender, EventArgs e)
        {
            lblSelectBestPerformingProducts.Text = "Top 5 best selling products:";
            populateChart(5);
            populateDataGridView(5);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            switchBestSellingProductView();
        }

        private void txtDeleteCustomerContactNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtCustomerVehicleYear_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtCustomerVehicleYear_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtUpdateCustomerVehicleDetailsFilledYear_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private bool validateEmail(TextBox txtEmail)
        {
            bool validEmail = true;

            //Email validation pattern
            string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";

            //Create a Regex object with the pattern
            Regex regex = new Regex(pattern);

            //Match the email against the declared pattern
            Match match = regex.Match(txtEmail.Text);

            //Check if email is valid
            validEmail = match.Success;

            //Return result
            return validEmail;
        }

        private void txtCustomerContactNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtUpdateCustomerDetailsFilledContactNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtViewAllVehiclesYear_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtUpdateCustomerContactNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtCustomerContactNumberFilter_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtRepairClientNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtPurchaseClientContactNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtServiceCustomersNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtRefundContactNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void tbCNumberSupp_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void cbxSupplierStock_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tbPriceProductStock_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtContactNumber_Update_User_Details_panel_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtContact_Number_pnlRemove_Users_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtContactNumber_AddUsers_panel_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void populateDeleteStockTextBoxes()
        {
            // Check if any row is selected
            if (dgvProductStock.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dgvProductStock.SelectedRows[0];

                // Access the cell values from the selected row using column indexes
                cbxSupplierStock.Text = selectedRow.Cells["Supplier_Name"].Value.ToString();
                tbProductName.Text = selectedRow.Cells["Product_Name"].Value.ToString();
                tbProductNumber.Text = selectedRow.Cells["Product_Number"].Value.ToString();
                tbPriceProductStock.Text = selectedRow.Cells["Unit_Price"].Value.ToString();
                sedAdd.Text = selectedRow.Cells["Available_Quantity"].Value.ToString();


                btnDeleteCustomer.Enabled = true;
            }


        }

        private void dgvProductStock_SelectionChanged(object sender, EventArgs e)
        {
            populateDeleteStockTextBoxes();
        }

        private void dgvProductStock_SizeChanged(object sender, EventArgs e)
        {

        }

        private void tbPriceProductStock_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtRepairClientNum_TextChanged(object sender, EventArgs e)
        {

        }
    }

}
