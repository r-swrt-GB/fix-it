using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace FIX_IT_Workshop
{
    public partial class Form1 : Form
    {

        //Declare SqlControls
        private SqlConnection conn;
        private SqlCommand command;
        private SqlDataReader dataReader;

        int userId;

        //Declare connectionString global
        public String connectionString;

        public Form1()
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

        private void clearInputFields()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtUsername.Focus();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            clearInputFields();
        }

        private bool validateForm()
        {
            bool valid = true;

            //Check if all textfields are populated
            if (txtPassword.Text == "" || txtUsername.Text == "")
            {
                valid = false;
            }

            //Return bool value based on user input
            return valid;
        }

        private void showHomePage()
        {
            Homepage homepage = new Homepage(userId);
            homepage.Show();

            this.Hide();
        }

        private void loginUser(String username, String password)
        {
            bool validUser = false;
            string firstName = "";
            string lastName = "";
            try
            {
                //Assign new connection
                conn = new SqlConnection(connectionString);

                // Open connection to the DB
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                //Select all records in the table
                string sql = "SELECT User_ID, Username, Password, First_Name, Last_Name FROM [User]";

                // Initialize new Sql command
                command = new SqlCommand(sql, conn);

                // Execute command
                dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    if (dataReader.GetValue(1).ToString() == username)
                    {
                        if (dataReader.GetValue(2).ToString() == password)
                        {
                            validUser = true;
                            userId = int.Parse(dataReader.GetValue(0).ToString());
                            firstName = (dataReader.GetValue(3).ToString());
                            lastName = (dataReader.GetValue(4).ToString());
                            break;
                        }
                    }
                }

                // Close conenction to DB
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                //TODO: REMOVE LINE BELOW BEFORE SUMBITING!!!
                validUser = true;

                if (validUser)
                {
                    // Display suitable error dialog
                    MessageBox.Show($"Login successful.\nWelcome back " + firstName + " " + lastName + "!");
                    showHomePage();
                }
                else
                {
                    // Display suitable error dialog
                    MessageBox.Show("Invalid login credentials.");
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

        private void btnLogIn_Click(object sender, EventArgs e)
        {
            //Check if user has entered all values
            bool isValidated = validateForm();


            //TODO: REMOVE LINE BELOW BEFORE SUMBITING!!!
            isValidated = true;
            if (isValidated)
            {
                //Attempt to log user in
                loginUser(txtUsername.Text, txtPassword.Text);
            }
            else
            {
                //Diplsay appropiate message
                MessageBox.Show("Please fill out all fields before continuing");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connectDatabase();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
