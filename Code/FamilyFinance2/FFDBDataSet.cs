﻿using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Data.SqlServerCe;
using System.Collections.Generic;
using FamilyFinance2.FFDBDataSetTableAdapters;

namespace FamilyFinance2 
{
    public partial class FFDBDataSet 
    {

        ///////////////////////////////////////////////////////////////////////
        //   Functions STATIC 
        ///////////////////////////////////////////////////////////////////////
        static public void myExecuteFile(string fileAsString, bool catchExceptions)
        {
            SqlCeConnection connection;
            SqlCeCommand command;
            string scriptLine = "";
            int start = 0;
            int end = 0;

            // Remove all comments
            while (true)
            {
                start = fileAsString.IndexOf("--", 0);
                if (start == -1)
                    break;

                end = fileAsString.IndexOf("\n", start) + 1;
                fileAsString = fileAsString.Remove(start, end - start);
            }

            // Replace all the white space characters
            fileAsString = fileAsString.Replace("\n", "");
            fileAsString = fileAsString.Replace("\r", "");
            fileAsString = fileAsString.Replace("\t", " ");


            connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            connection.Open();



            try 
            {
                while (true)
                {
                    // Find the next statments end
                    end = fileAsString.IndexOf(";", 0) + 1;
                    if (end == 0)
                        break;

                    scriptLine = fileAsString.Substring(0, end);
                    fileAsString = fileAsString.Remove(0, end);

                    // Execute the statement
                    command = new SqlCeCommand(scriptLine, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Caught a bad SQL Line <" + scriptLine + ">", e);
            }
            finally
            {
                connection.Close();
            }
            
        }

        static public bool myCreateDBFile()
        {
            SqlCeEngine engine = new SqlCeEngine();
            string connection = Properties.Settings.Default.FFDBConnectionString;
            string filePath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            filePath += "\\" + Properties.Settings.Default.DBFileName;

            if (File.Exists(filePath))
                return false;

            engine.LocalConnectionString = connection;

            try
            {
                engine.CreateDatabase();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to make the DBFile", e);
            }
            finally
            {
                engine.Dispose();
            }

            myExecuteFile(Properties.Resources.Build_Tables, true);

            return true;
        }

        static public bool myGoodPath()
        {
            SqlCeConnection connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            bool result;

            try
            {
                connection.Open();

                if (connection.State == ConnectionState.Open)
                    result = true;
                else
                    result = false;
            }
            catch (Exception e)
            {
                string temp = e.Message;
                result = false;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        static public void myResetAccountBalances()
        {
            AccountDataTable account = new AccountDataTable();
            AccountTableAdapter accTA = new AccountTableAdapter();
            List<AccountSums> sums = new List<AccountSums>();

            // Bring up the Account table with the ending and current balances Zeroed out.
            accTA.FillByZero(account);

            // Get the endingBalance Sums
            sums = FFDBDataSet.myGetAccountSums();

            // Reset the endingBalances
            foreach (AccountSums sumRow in sums)
            {
                if (account.FindByid(sumRow.accountID).creditDebit == LineCD.DEBIT)
                {
                    // If this in a Debit account (Checking, Savings) then subtract the Credits and add the debits
                    if (sumRow.creditDebit == LineCD.CREDIT)
                        account.FindByid(sumRow.accountID).endingBalance -= sumRow.balance;
                    else
                        account.FindByid(sumRow.accountID).endingBalance += sumRow.balance;
                }
                else
                {
                    // Else this is a Credit account (Loan, Credit) then add the Credits and subtract the debits
                    if (sumRow.creditDebit == LineCD.CREDIT)
                        account.FindByid(sumRow.accountID).endingBalance += sumRow.balance;
                    else
                        account.FindByid(sumRow.accountID).endingBalance -= sumRow.balance;
                }
            }

            // Get tomorrows Date and the currentBalance Sums
            DateTime tomorrow = DateTime.Today;
            tomorrow.AddDays(1.0);
            sums.Clear();
            sums = FFDBDataSet.myGetAccountSumsBeforeDate(tomorrow);

            // Reset the currentBalances
            foreach (AccountSums sumRow in sums)
            {
                if (account.FindByid(sumRow.accountID).creditDebit == LineCD.DEBIT)
                {
                    // If this in a Debit account (Checking, Savings) then subtract the Credits and add the Debits
                    if (sumRow.creditDebit == LineCD.CREDIT)
                        account.FindByid(sumRow.accountID).currentBalance -= sumRow.balance;
                    else
                        account.FindByid(sumRow.accountID).currentBalance += sumRow.balance;
                }
                else
                {
                    // Else this is a Credit account (Loan, Credit) then add the Credits and subtract the Debits
                    if (sumRow.creditDebit == LineCD.CREDIT)
                        account.FindByid(sumRow.accountID).currentBalance += sumRow.balance;
                    else
                        account.FindByid(sumRow.accountID).currentBalance -= sumRow.balance;
                }
            }

            // Save back to the database and dispose of the temperary tables and adapters.
            accTA.Update(account);
            account.Dispose();
            sums.Clear();
        }

        static public void myResetEnvelopeBalances()
        {
            EnvelopeDataTable envelope;
            List<EnvelopeSums> sums;
            EnvelopeTableAdapter envTA = new EnvelopeTableAdapter();

            // Bring up the Envelope table with the ending and current balances Zeroed out.
            envelope = envTA.GetDataByZero();

            // Get the endingBalance Sums
            sums = FFDBDataSet.myGetEnvelopeSums();

            // Reset the endingBalances
            foreach (EnvelopeSums balRow in sums)
            {
                // For Envelopes subtract the Credits and add the Debits
                if (balRow.creditDebit == LineCD.CREDIT)
                    envelope.FindByid(balRow.envelopeID).endingBalance -= balRow.balance;
                else
                    envelope.FindByid(balRow.envelopeID).endingBalance += balRow.balance;
            }

            // Get tomorrows Date and the currentBalance Sums
            DateTime tomorrow = DateTime.Today;
            tomorrow.AddDays(1.0);
            sums.Clear();
            sums = FFDBDataSet.myGetEnvelopeSumsBeforeDate(tomorrow);

            // Reset the currentBalances
            foreach (EnvelopeSums balRow in sums)
            {
                // Subtract the Credits and add the Debits
                if (balRow.creditDebit == LineCD.CREDIT)
                    envelope.FindByid(balRow.envelopeID).currentBalance -= balRow.balance;
                else
                    envelope.FindByid(balRow.envelopeID).currentBalance += balRow.balance;
            }

            // Save back to the database and dispose of the temperary tabls and adapters.
            envTA.Update(envelope);
            envelope.Dispose();
            sums.Clear();
        }

        static public void myResetAEBalance()
        {
            AEBalanceDataTable aeBalance;
            AEBalanceTableAdapter aeTA = new AEBalanceTableAdapter();
            List<AEBalanceSums> balances;

            // Delete the AEBalance Table
            aeTA.DeleteQuery();
            aeBalance = aeTA.GetData();

            // Get the endingBalance Sums
            balances = FFDBDataSet.myGetAEBalanceSums();

            // Reset the endingBalances
            foreach (AEBalanceSums balRow in balances)
            {
                // For AEBalances subtract the Credits and add the Debits
                if (balRow.creditDebit == LineCD.CREDIT)
                    aeBalance.myGetRow(balRow.accountID, balRow.envelopeID).endingBalance -= balRow.balance;
                else
                    aeBalance.myGetRow(balRow.accountID, balRow.envelopeID).endingBalance += balRow.balance;
            }

            // Get tomorrows Date and the currentBalance Sums
            DateTime tomorrow = DateTime.Today;
            tomorrow.AddDays(1.0);
            balances.Clear();
            balances = FFDBDataSet.myGetAEBalanceSumsBeforeDate(tomorrow);

            // Reset the currentBalances
            foreach (AEBalanceSums balRow in balances)
            {
                // Subtract the Credits and add the Debits
                if (balRow.creditDebit == LineCD.CREDIT)
                    aeBalance.myGetRow(balRow.accountID, balRow.envelopeID).currentBalance -= balRow.balance;
                else
                    aeBalance.myGetRow(balRow.accountID, balRow.envelopeID).currentBalance += balRow.balance;
            }

            // Save back to the database and dispose of the temperary tabls and adapters.
            aeTA.Update(aeBalance);
            aeBalance.Dispose();
            balances.Clear();
        }

        static private List<int> myDBGetIDList(string col, string table, string filter, string sort)
        {
            SqlCeConnection connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            List<int> idList = new List<int>();
            SqlCeCommand selectCmd;
            SqlCeDataReader reader;
            string command;

            connection.Open();

            command = "SELECT DISTINCT " + col;
            command += " FROM " + table;

            if (filter != "")
                command += " WHERE " + filter;

            if (sort != "")
                command += " ORDER BY " + sort;

            command += " ;";

            selectCmd = new SqlCeCommand(command, connection);
            reader = selectCmd.ExecuteReader();

            while (reader.Read())
                idList.Add(reader.GetInt32(0));

            reader.Close();
            connection.Close();

            return idList;
        }

        static private int myDBGetNewID(string col, string table)
        {
            SqlCeConnection connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            SqlCeCommand selectCmd = new SqlCeCommand();
            object result;
            int num;

            connection.Open();

            selectCmd.Connection = connection;
            selectCmd.CommandText = "SELECT MAX(" + col + ") FROM " + table + " ;";
            result = selectCmd.ExecuteScalar();

            connection.Close();

            if (result == DBNull.Value)
                return 1;

            num = Convert.ToInt32(result);

            if (num <= 0)
                return 1;

            return num + 1;
        }

        static private decimal myDBGetSubSum(int lineID, out int subCount, out short envelopeID)
        {
            SqlCeConnection connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            SqlCeCommand selectCmd = new SqlCeCommand();
            SqlCeDataReader reader;
            decimal sum;

            sum = 0.0m;
            subCount = 0;
            envelopeID = SpclEnvelope.NULL;

            connection.Open();

            selectCmd.Connection = connection;
            selectCmd.CommandText = "SELECT amount, envelopeID FROM SubLineItem WHERE lineItemID = " + lineID.ToString() + ";";
            reader = selectCmd.ExecuteReader();

            while (reader.Read())
            {
                sum += reader.GetDecimal(0);
                subCount++;
                envelopeID = reader.GetInt16(1);
            }

            if (subCount > 1)
                envelopeID = SpclEnvelope.SPLIT;

            reader.Close();
            connection.Close();

            return sum;
        }


        ///////////////////////////////////////////////////////////////////////
        //   Functions Private 
        ///////////////////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////////////////
        //   Functions Public 
        ///////////////////////////////////////////////////////////////////////
        public void myCheckTransactionInTable(int transID)
        {
            List<int> lineIDList = new List<int>();
            decimal creditSum = 0.0m;
            decimal debitSum = 0.0m;
            bool transError;
            short creditOppAccountID = SpclAccount.NULL;
            short debitOppAccountID = SpclAccount.NULL;
            int creditCount = 0;
            int debitCount = 0;

            // Gather information from the transaction
            foreach (LineItemRow line in this.LineItem)
            {
                if (line.RowState != DataRowState.Deleted && line.transactionID == transID && line.RowState != DataRowState.Detached)
                {
                    if (line.creditDebit == LineCD.CREDIT)
                    {
                        lineIDList.Add(line.id);
                        creditCount++;
                        creditSum += line.amount;
                        debitOppAccountID = line.accountID; // This is usefull when there is only one credit
                    }
                    else
                    {
                        lineIDList.Add(line.id);
                        debitCount++;
                        debitSum += line.amount;
                        creditOppAccountID = line.accountID; // This is usefull when there is only one debit
                    }
                }
            }

            // If there are no lines, there is nothing to do.
            if (creditCount == 0 && debitCount == 0)
                return;

            // Determine if there is a transaction error.
            transError = (creditSum != debitSum);

            // Determine the oppAccount values for complex transactions
            if (creditCount > 1)
                debitOppAccountID = SpclAccount.MULTIPLE;

            if (debitCount > 1)
                creditOppAccountID = SpclAccount.MULTIPLE;


            // Check each line for Line errors and set transaction errors
            foreach (int id in lineIDList)
            {
                int subCount;
                short envelopeID;
                decimal subSum = this.SubLineItem.mySubLineSum(id, out subCount, out envelopeID);
                LineItemRow line = this.LineItem.FindByid(id);
                bool usesEnvelopes = line.AccountRowByFK_Line_accountID.envelopes;
                bool lineError;

                // Determine if there is a lineError and set the value.
                if ((usesEnvelopes && line.amount == subSum) || (!usesEnvelopes && subSum == 0.0m))
                    lineError = false;
                else
                    lineError = true;

                // Set the Line Error if needed
                if (line.lineError != lineError)
                    line.lineError = lineError;

                // Set the Transaction Error if needed
                if (line.transactionError != transError)
                    line.transactionError = transError;

                // Set the OppacountID if needed
                if (line.creditDebit == LineCD.CREDIT)
                {
                    if (line.oppAccountID != creditOppAccountID)
                        line.oppAccountID = creditOppAccountID;
                }
                else
                {
                    if (line.oppAccountID != debitOppAccountID)
                        line.oppAccountID = debitOppAccountID;
                }

                // Set the EnvelopeID if needed
                if (line.envelopeID != envelopeID)
                    line.envelopeID = envelopeID;
            }
        }

        public void myCheckTransactionInDataBase(int lineID, int transID)
        {

            
        }

        public void mySaveTransaction()
        {
            this.LineItem.mySaveNewLines();
            this.SubLineItem.mySaveChanges();
            this.LineItem.mySaveChanges();
        }


    }
}