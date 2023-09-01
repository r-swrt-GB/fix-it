using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace FIX_IT_Workshop
{
    public partial class Homepage : Form
    {
        Color selectedLabelColour = Color.FromArgb(180, 184, 171);
        Label currentlySelectedLabel;

        private SqlDataAdapter adap;
        private DataSet ds;
        public string connstr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|FixItDatabase.mdf;Integrated Security=True";

        //Declare SqlControls
        private SqlConnection conn;
        private SqlCommand command;
        private SqlDataReader dataReader;
        private SqlDataAdapter dataAdapter;

        int userId;
        int customerPrimaryKey = -1;

        //Declare connectionString global
        public String connectionString;

        public Homepage()
        {
            InitializeComponent();
        }

        private void connectDatabase()
        {
            //Initialize connectionString
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
            tbcHomepage.SelectedIndex = -1;
            selectLabel(lblLogOut);
            confirmLogout();
        }

        private void lblShop_Click(object sender, EventArgs e)
        {
            tbcHomepage.SelectedTab = tbpShop;
            showNewSalesPanel(pnlSaleChoice);
            selectLabel(lblShop);

        }

        private void lblBookings_Click(object sender, EventArgs e)
        {
            tbcHomepage.SelectedTab = tbpBookings;
            selectLabel(lblSupplier);
            conn = new SqlConnection(connstr);
            conn.Open();
            adap = new SqlDataAdapter();
            ds = new DataSet();
            string sql = "SELECT Name,Contact_Number,Email FROM Supplier";
            command = new SqlCommand(sql, conn);
            adap.SelectCommand = command;
            adap.Fill(ds, "Supplier");
            dgvSupp.DataSource = ds;
            dgvSupp.DataMember = "Supplier";
            conn.Close();
        }

        private void lblOrders_Click(object sender, EventArgs e)
        {
            tbcHomepage.SelectedTab = tbpOrders;
            selectLabel(lblOrders);
        }

        private void lblStock_Click(object sender, EventArgs e)
        {
            tbcHomepage.SelectedTab = tbpStock;
            selectLabel(lblStock);
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
            if (verifyCustomerDetails())
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
                string sql = $"INSERT INTO Vehicle (Make, Model, Year, License_Plate_Number, Client_ID) VALUES (@make, @model, @year, @lisence_plate_number, @customer_id)";
                command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@make", make);
                command.Parameters.AddWithValue("@model", model);
                command.Parameters.AddWithValue("@year", year);
                command.Parameters.AddWithValue("@lisence_plate_number", licensePlateNumber);
                command.Parameters.AddWithValue("@customer_id", customerId);
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
                MessageBox.Show("Sign up failed.\nPlease try again later.");

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
                MessageBox.Show(customerPrimaryKey.ToString());
                deleteRecord($"DELETE FROM Client WHERE First_Name = '{txtDeleteCustomerFirstName.Text}' AND Last_Name = '{txtDeleteCustomerLastName.Text}' AND Contact_Number = '{txtDeleteCustomerContactNumber.Text}' AND Email = '{txtDeleteCustomerEmail.Text}'", false);
                deleteRecord($"DELETE FROM Vehicle WHERE Client_ID = {customerPrimaryKey}", true);
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

                // Initialize new Sql zommand
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
            //tbcHomepage is a TabControl 

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
            tbcHomepage.SelectedTab = tbpAddCustomer;
            showNewCustomerPanel(pnlCustomerOptions);
            selectLabel(lblAddCustomer);
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
            pnlView_All_Users_panel.BringToFront();
        }

        private void btnRemove_Users_Click_1(object sender, EventArgs e)
        {
            showNewUserPanel(pnlRemove_Users);
            pnlRemove_Users.BringToFront();
        }

        private void btnUpdate_User_Details_Click_1(object sender, EventArgs e)
        {
            showNewUserPanel(pnlUpdate_User_Details);
            pnlUpdate_User_Details.BringToFront();
        }

        private void btnAdd_New_Users_Click_1(object sender, EventArgs e)
        {
            showNewUserPanel(pnlAdd_New_Users);
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
            conn = new SqlConnection(connstr);
            conn.Open();

            ds = new DataSet();
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
            ds = new DataSet();
            sql = "SELECT Name,Contact_Number,Email FROM Supplier";
            command = new SqlCommand(sql, conn);
            adap.SelectCommand = command;
            adap.Fill(ds, "Supplier");
            dgvSupp.DataSource = ds;
            dgvSupp.DataMember = "Supplier";
            conn.Close();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            conn = new SqlConnection(connstr);
            conn.Open();
            adap = new SqlDataAdapter();
            ds = new DataSet();
            string sql = $"SELECT Name, Email, Contact_Number FROM Supplier WHERE UPPER(Name) LIKE '%{tbNameSupp.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{tbEmailSupp.Text.ToUpper()}%' AND (Contact_Number) LIKE '%{tbCNumberSupp.Text}%'";
            command = new SqlCommand(sql, conn);
            adap.SelectCommand = command;
            adap.Fill(ds, "Supplier");
            dgvSupp.DataSource = ds;
            dgvSupp.DataMember = "Supplier";
            conn.Close();
        }

        private void btnDeleteSupp_Click(object sender, EventArgs e)
        {
            conn = new SqlConnection(connstr);
            conn.Open();

            ds = new DataSet();
            string sql = $"DELETE FROM Supplier Where Name ='{dgvSupp[dgvSupp.CurrentRow.Index, 0].Value}'";

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
            ds = new DataSet();
            sql = "SELECT Name,Contact_Number,Email FROM Supplier";
            command = new SqlCommand(sql, conn);
            adap.SelectCommand = command;
            adap.Fill(ds, "Supplier");
            dgvSupp.DataSource = ds;
            dgvSupp.DataMember = "Supplier";
            conn.Close();
        }

        private void btnClearFilterSupp_Click(object sender, EventArgs e)
        {
            tbCNumberSupp.Clear();
            tbEmailSupp.Clear();
            tbNameSupp.Clear();
        }

        private void tbEmailSupp_TextChanged(object sender, EventArgs e)
        {
            conn = new SqlConnection(connstr);
            conn.Open();
            adap = new SqlDataAdapter();
            ds = new DataSet();
            string sql = $"SELECT Name, Email, Contact_Number FROM Supplier WHERE UPPER(Name) LIKE '%{tbNameSupp.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{tbEmailSupp.Text.ToUpper()}%' AND (Contact_Number) LIKE '%{tbCNumberSupp.Text}%'";
            command = new SqlCommand(sql, conn);
            adap.SelectCommand = command;
            adap.Fill(ds, "Supplier");
            dgvSupp.DataSource = ds;
            dgvSupp.DataMember = "Supplier";
            conn.Close();
        }

        private void tbCNumberSupp_TextChanged(object sender, EventArgs e)
        {
            conn = new SqlConnection(connstr);
            conn.Open();
            adap = new SqlDataAdapter();
            ds = new DataSet();
            string sql = $"SELECT Name, Email, Contact_Number FROM Supplier WHERE UPPER(Name) LIKE '%{tbNameSupp.Text.ToUpper()}%' AND UPPER(Email) LIKE '%{tbEmailSupp.Text.ToUpper()}%' AND (Contact_Number) LIKE '%{tbCNumberSupp.Text}%'";
            command = new SqlCommand(sql, conn);
            adap.SelectCommand = command;
            adap.Fill(ds, "Supplier");
            dgvSupp.DataSource = ds;
            dgvSupp.DataMember = "Supplier";
            conn.Close();
        }
        //

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
        }

        private void btnNavOrderFromSupplier_Click_1(object sender, EventArgs e)
        {
            showNewSalesPanel(pnlOrderStock);
            pnlOrderStock.BringToFront();
        }

        private void btnNavChangeSales_Click(object sender, EventArgs e)
        {
            showNewSalesPanel(pnlChangeSale);
            pnlChangeSale.BringToFront();
        }

        private void btnNavTransaction_Click_1(object sender, EventArgs e)
        {
            showNewSalesPanel(pnlViewTransaction);
            pnlViewTransaction.BringToFront();
        }

        private void btnNavRepair_Click_1(object sender, EventArgs e)
        {
            showNewSalesPanel(pnlRepair);
            pnlRepair.BringToFront();
        }

        private void btnNavSell_Click_1(object sender, EventArgs e)
        {
            showNewSalesPanel(pnlMakePurcahes);
            pnlMakePurcahes.BringToFront();
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
                String sql = $"SELECT Client_ID FROM Client WHERE UPPER(Email)  = '{email.ToUpper()}' AND UPPER(First_Name)  = '{firstName.ToUpper()}' AND UPPER(Last_Name)  = '{lastName.ToUpper()}' AND Contact_Number  = '{contactNumber}'";

                // Initialize new Sql command
                command = new SqlCommand(sql, conn);

                // Execute command
                dataReader = command.ExecuteReader();



                while (dataReader.Read())
                {
                    customerPrimaryKey = dataReader.GetInt32(0);
                    MessageBox.Show("Hi");
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
                String sql = $"SELECT Make, Model, Year, License_Plate_Number FROM Vehicle WHERE Client_ID = {customerId}";

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
            showNewCustomerPanel(pnlUpdateCustomerDetailsFilled);
            txtUpdateCustomerDetailsFilledFirstName.Text = txtUpdateCustomerFirstName.Text;
            txtUpdateCustomerDetailsFilledLastName.Text = txtUpdateCustomerLastName.Text;
            txtUpdateCustomerDetailsFilledEmail.Text = txtUpdateCustomerEmail.Text;
            txtUpdateCustomerDetailsFilledContactNumber.Text = txtUpdateCustomerContactNumber.Text;

            setCustomerPrimaryKey(txtUpdateCustomerDetailsFilledFirstName.Text, txtUpdateCustomerDetailsFilledLastName.Text, txtUpdateCustomerDetailsFilledEmail.Text, txtUpdateCustomerDetailsFilledContactNumber.Text);
            setCustomerUpdateVehicleFields(customerPrimaryKey);

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
            else
            {
                btnUpdateCustomerDetailsConfirm.Enabled = false;
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
            showNewCustomerPanel(pnlUpdateCustomerVehicleDetailsFilled);
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
            updateRecord($"UPDATE Vehicle SET Make = '{make}', Model = '{model}', Year = '{year}', License_Plate_Number = '{licensePlate}' WHERE Client_ID = ${customerPrimaryKey}");
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
    }
}
