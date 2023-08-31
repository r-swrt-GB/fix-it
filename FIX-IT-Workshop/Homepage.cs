using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FIX_IT_Workshop
{
    public partial class Homepage : Form
    {
        Color selectedLabelColour = Color.FromArgb(180, 184, 171);
        Label currentlySelectedLabel;

        public Homepage()
        {
            InitializeComponent();
        }

        public void resetAllLabels()
        {
            lblAddCustomer.ForeColor = Color.White;
            lblBookings.ForeColor = Color.White;
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

        private void label7_Click(object sender, EventArgs e)
        {
            tbcHomepage.SelectedTab = tbpAddCustomer;
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
            tbcHomepage.SelectedTab = tbpStock;
            selectLabel(lblShop);
        }

        private void lblBookings_Click(object sender, EventArgs e)
        {
            tbcHomepage.SelectedTab = tbpBookings;
            selectLabel(lblBookings);
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
            selectLabel(lblBookings);
        }

        private void lblBookings_MouseLeave(object sender, EventArgs e)
        {
            deselectLabel(lblBookings);
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

        private void btnCustomerVehicleInfoFinish_Click(object sender, EventArgs e)
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


                //Display appropiate message to the user
                MessageBox.Show($"{firstName} {lastName} has been successfully registered.");

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

        private void btnCustomerVehicleInfoBack_Click(object sender, EventArgs e)
        {
            
            pnlCustomerDetails.BringToFront();
          
            showNewCustomerPanel(pnlCustomerDetails);
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

        public void resetDeleteCustomerFilter()
        {
            txtCustomerFirstNameFilter.Clear();
            txtCustomerLastNameFilter.Clear();
            txtCustomerEmailFilter.Clear();
            txtCustomerContactNumberFilter.Clear();
        }

        private void btnDeleteCustomer_Click(object sender, EventArgs e)
        {
            resetDeleteCustomerFilter();
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

        private void btnViewAllCustomers_Click(object sender, EventArgs e)
        {
            showNewCustomerPanel(pnlCustomerViewAll);
            pnlCustomerViewAll.BringToFront();
        }

        private void btnRemoveCustomer_Click(object sender, EventArgs e)
        {
            showNewCustomerPanel(pnlDeleteCustomer);
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
            showNewCustomerPanel(pnlUpdateCustomerDetails);
            pnlUpdateCustomerDetails.BringToFront();
        }

        private void label6_Click(object sender, EventArgs e)
        {
            tbcHomepage.SelectedTab = tbpUsers;
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

            selectedPanel.Visible = true;
        }

        private void Homepage_Load(object sender, EventArgs e)
        {
            tbcHomepage.SelectedTab = tbpAddCustomer;
            showNewCustomerPanel(pnlCustomerOptions);
            selectLabel(lblAddCustomer);
        }
        private void btnView_All_Users_Click(object sender, EventArgs e)
        {

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
            txtFirst_Name.Clear();
            txtLast_Name.Clear();

            cBUsers.SelectedIndex = -1;
        }
    }
}
