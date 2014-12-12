﻿using System;
using System.DirectoryServices.Protocols;
using System.Drawing;
using System.Windows.Forms;
using GUI_LDAPUnitTest.Tests.BusinessLogic;
using GUI_LDAPUnitTest.Tests.GUIStructures;
using LDAPLibrary;
using LDAPLibrary.Interfarces;

namespace GUI_LDAPUnitTest.Forms
{
    public partial class TestForm : Form
    {
        private readonly TestManager _testManagerObj;
        private readonly TestTripletRepository _testTripletRepository;

        public TestForm()
        {
            InitializeComponent();
            try
            {
                _testTripletRepository = new TestTripletRepository();

                AddTestTripletsToRepository();

                _testManagerObj = new TestManager(SetUpLdapLibrary());

                currentUserLabel.Text = _testManagerObj.UserRepository.GetTestUserCn();

                _testTripletRepository.SetAllStateLabelText(Constants.TestLableUndefined);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format(
                        "Cannot load the Config - Standard LDAP Library Init failed{0}Exception Message: {1}",
                        Environment.NewLine, ex.Message),
                    @"Error Config Loading",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        #region InterfaceEvents

        #region Menu Events

        /// <summary>
        ///     Application exit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion

        #region Setup Buttons

        private void configModifyUserDescriptionButton_Click(object sender, EventArgs e)
        {
            using (
                var setupModifyUserDescriptionForm =
                    new OneItemConfigurationForm(OneItemConfigurationState.NewDescription,
                        _testManagerObj.UserRepository))
            {
                setupModifyUserDescriptionForm.ShowDialog();
            }
        }

        private void configSearchUserButton_Click(object sender, EventArgs e)
        {
            using (
                var setupSearchUserForm = new OneItemConfigurationForm(OneItemConfigurationState.UserToSearch,
                    _testManagerObj.UserRepository))
            {
                setupSearchUserForm.ShowDialog();
            }
        }

        private void configUserChangePasswordButton_Click(object sender, EventArgs e)
        {
            using (
                var setupNewPasswordForm = new OneItemConfigurationForm(OneItemConfigurationState.NewPassword,
                    _testManagerObj.UserRepository))
            {
                setupNewPasswordForm.ShowDialog();
            }
        }

        /// <summary>
        ///     launch the newTestUser form and update the main form at return.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setUserButton_Click(object sender, EventArgs e)
        {
            using (var testUserForm = new TestUserForm(_testManagerObj.UserRepository))
            {
                testUserForm.ShowDialog();
            }
            currentUserLabel.Text = _testManagerObj.UserRepository.GetTestUserCn();
        }

        #endregion

        #region ComboBox Visibility Events

        private void testCreateUserCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            testCreateUserLabel.Enabled = testCreateUserCheckBox.Checked;
            stateCreateUserLabel.Enabled = testCreateUserCheckBox.Checked;
        }

        private void testModifyUserDescriptionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            testModifyUserDescriptionLabel.Enabled = testModifyUserDescriptionCheckBox.Checked;
            stateModifyUserDescriptionLabel.Enabled = testModifyUserDescriptionCheckBox.Checked;
        }

        private void testSearchUserCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            testSearchUserLabel.Enabled = testSearchUsersCheckBox.Checked;
            stateSearchUsersLabel.Enabled = testSearchUsersCheckBox.Checked;
        }

        private void testConnectUserCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            testConnectUserLabel.Enabled = testConnectUserCheckBox.Checked;
            stateConnectUserLabel.Enabled = testConnectUserCheckBox.Checked;
        }

        private void testSearchUserAndConnectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            testSearchUserAndConnectLabel.Enabled = testSearchUserAndConnectCheckBox.Checked;
            stateSearchUserAndConnectLabel.Enabled = testSearchUserAndConnectCheckBox.Checked;
        }

        private void testUserChangePasswordCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            testUserChangePasswordLabel.Enabled = testUserChangePasswordCheckBox.Checked;
            stateUserChangePasswordLabel.Enabled = testUserChangePasswordCheckBox.Checked;
        }

        private void testDeleteUserCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            testDeleteUserLabel.Enabled = testDeleteUserCheckBox.Checked;
            stateDeleteUserLabel.Enabled = testDeleteUserCheckBox.Checked;
        }

        #endregion

        #region Other Events

        /// <summary>
        ///     Manage the visibility and repositioning of the panels.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void readTestsLabel_Click(object sender, EventArgs e)
        {
            if (readTestsPanel.Height == 35)
            {
                //Expand the read and move the write panel
                readTestsPanel.Height = 180;
                readTestIconLabel.Text = @"-";
                writeTestsPanel.Location = new Point(writeTestsPanel.Location.X, writeTestsPanel.Location.Y + 150);
            }
            else if (readTestsPanel.Height == 180)
            {
                //Collapse the read panel and write panel repositioned.
                readTestsPanel.Height = 35;
                readTestIconLabel.Text = @"+";
                writeTestsPanel.Location = new Point(writeTestsPanel.Location.X, writeTestsPanel.Location.Y - 150);
            }
        }


        private void WriteTestsLabel_Click(object sender, EventArgs e)
        {
            if (writeTestsPanel.Height == 35)
            {
                writeTestsPanel.Height = 220;
                writeTestsIconLabel.Text = @"-";
            }
            else
            {
                writeTestsPanel.Height = 35;
                writeTestsIconLabel.Text = @"+";
            }
        }

        #endregion

        /// <summary>
        ///     Start the tests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void runButton_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            testsProgressBar.Value = 0;
            _testTripletRepository.SetAllStateLabelText(Constants.TestLableInProgress);

            int progressBarIncrement = 100/Enum.GetNames(typeof (TestType)).Length;
            try
            {
                foreach (TestTriplet t in _testTripletRepository.TestTripletList)
                {
                    if (t.TestCheckbox.Checked)
                    {
                        t.TestLabel.Text = Constants.TestLableStarted;
                        if (_testManagerObj.RunTest(t.TestType))
                        {
                            t.TestLabel.Text = Constants.TestLablePassed;
                            t.TestLabel.ForeColor = Constants.TestPassedColorLable;
                        }
                        else
                        {
                            t.TestLabel.Text = Constants.TestLableFailed;
                            t.TestLabel.ForeColor = Constants.TestFailedColorLable;
                        }
                    }
                    else
                    {
                        t.TestLabel.Text = Constants.TestLableSkipped;
                    }
                    testsProgressBar.Value += progressBarIncrement;
                }
            }
            catch (Exception)
            {
                MessageBox.Show(@"Qualcosa non ha funzionato nel programma di test");
            }

            testsProgressBar.Value = 100;


            Cursor.Current = Cursors.Default;
        }

        #endregion

        #region Utilities

        /// <summary>
        ///     Set up the LDAPLibrary with minimal configuration.
        /// </summary>
        private ILdapManager SetUpLdapLibrary()
        {
            var authType = (AuthType) Enum.Parse(typeof (AuthType),
                Config.LDAPLibrary["LDAPAuthType"]);

            if (!string.IsNullOrEmpty(Config.LDAPLibrary["LDAPAdminUserDN"]))
            {
                var adminUser = new LdapUser(Config.LDAPLibrary["LDAPAdminUserDN"],
                    Config.LDAPLibrary["LDAPAdminUserCN"],
                    Config.LDAPLibrary["LDAPAdminUserSN"],
                    null);

                adminUser.CreateUserAttribute("userPassword", Config.LDAPLibrary["LDAPAdminUserPassword"]);


                return new LdapManager(adminUser,
                    Config.LDAPLibrary["LDAPServer"],
                    Config.LDAPLibrary["LDAPSearchBaseDN"],
                    authType
                    );
            }
            return new LdapManager(null,
                Config.LDAPLibrary["LDAPServer"],
                Config.LDAPLibrary["LDAPSearchBaseDN"],
                authType
                );
        }

        /// <summary>
        ///     add all controls in relation each other like triplets.
        /// </summary>
        private void AddTestTripletsToRepository()
        {
            _testTripletRepository.AddTestTriplet(new TestTriplet(testStandardInitLibraryNoAdminCheckBox,
                TestType.TestStandardInitLibraryNoAdmin,
                stateStandardInitLibraryNoAdminLabel));

            _testTripletRepository.AddTestTriplet(new TestTriplet(testInitLibraryNoAdminCheckBox,
                TestType.TestInitLibraryNoAdmin,
                stateInitLibraryNoAdminLabel));

            _testTripletRepository.AddTestTriplet(new TestTriplet(testInitLibraryCheckBox,
                TestType.TestInitLibrary,
                stateInitLibraryLabel));

            _testTripletRepository.AddTestTriplet(new TestTriplet(testAdminConnectCheckBox,
                TestType.TestAdminConnection,
                stateAdminConnectLabel));

            _testTripletRepository.AddTestTriplet(new TestTriplet(testConnectUserCheckBox,
                TestType.TestConnectUser,
                stateConnectUserLabel));

            _testTripletRepository.AddTestTriplet(new TestTriplet(testSearchUserAndConnectCheckBox,
                TestType.TestSearchUserAndConnect,
                stateSearchUserAndConnectLabel));

            _testTripletRepository.AddTestTriplet(new TestTriplet(testSearchUsersCheckBox,
                TestType.TestSearchUsers,
                stateSearchUsersLabel));

            _testTripletRepository.AddTestTriplet(new TestTriplet(testCreateUserCheckBox,
                TestType.TestCreateUser,
                stateCreateUserLabel));

            _testTripletRepository.AddTestTriplet(new TestTriplet(testModifyUserDescriptionCheckBox,
                TestType.TestModifyUserDescription,
                stateModifyUserDescriptionLabel));

            _testTripletRepository.AddTestTriplet(new TestTriplet(testUserChangePasswordCheckBox,
                TestType.TestUserChangePassword,
                stateUserChangePasswordLabel));

            _testTripletRepository.AddTestTriplet(new TestTriplet(testDeleteUserCheckBox,
                TestType.TestDeleteUser,
                stateDeleteUserLabel));
        }

        #endregion
    }
}