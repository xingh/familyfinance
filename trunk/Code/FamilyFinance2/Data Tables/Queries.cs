using System;
using System.Text;
using System.Data;
using System.Data.SqlServerCe;
using System.Collections.Generic;

namespace FamilyFinance2
{

    #region Account Details View Query

    public class AccountBalanceDetails
    {
        public short accountID;
        public string accountName;
        public string typeName;
        public short typeID;
        public decimal currentBalance;
        public bool error;
    }

    partial class FFDBDataSet
    {
        static public List<AccountBalanceDetails> myGetAccountBalanceDetails(byte catagory)
        {
            List<AccountBalanceDetails> queryResults = new List<AccountBalanceDetails>();
            SqlCeConnection connection;
            SqlCeCommand command;
            string query;

            query = "  SELECT Account.id, Account.name, Account.accountTypeID, AccountType.name, Account.currentBalance, Errors.error ";
            query += " FROM AccountType INNER JOIN Account ON AccountType.id = Account.accountTypeID ";
		    query +=        " LEFT JOIN ";
		    query +=        " ( ";
			query +=            " SELECT DISTINCT(accountID), 1 AS error ";
			query +=            " FROM LineItem ";
			query +=            " WHERE transactionError = 1 OR lineError = 1 ";
		    query +=        " ) AS Errors ";
		    query +=        " ON Account.id = Errors.accountID ";
            query += " WHERE Account.closed = 0 AND Account.id > 0 AND Account.catagoryID = " + catagory.ToString();
            query += " ORDER BY AccountType.name, Account.name ";

            connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            command = new SqlCeCommand(query, connection);
            connection.Open();
            SqlCeDataReader reader = command.ExecuteReader();

            try
            {
                // Iterate through the results
                while (reader.Read())
                {
                    AccountBalanceDetails ad = new AccountBalanceDetails();
                    ad.accountID = reader.GetInt16(0);
                    ad.accountName = reader.GetString(1);
                    ad.typeID = reader.GetInt16(2);
                    ad.typeName = reader.GetString(3);
                    ad.currentBalance = reader.GetDecimal(4);
                    ad.error = !reader.IsDBNull(5); // Error column is NULL for no error and 1 for error

                    queryResults.Add(ad);
                }
            }
            finally
            {
                // Always call Close the reader and connection when done reading
                reader.Close();
                connection.Close();
            }

            return queryResults;
        }
    }

    #endregion Account Details Query


    #region Account Sums View Query

    public class AccountSums
    {
        public short accountID;
        public bool creditDebit;
        public decimal balance;
    }

    partial class FFDBDataSet
    {
        static public List<AccountSums> myGetAccountSums()
        {
            List<AccountSums> queryResults = new List<AccountSums>();
            SqlCeConnection connection;
            SqlCeCommand command;
            string query;

            query = "  SELECT accountID, creditDebit, SUM(amount) as [sum]";
            query += " FROM LineItem ";
            query += " GROUP BY accountID, creditDebit ";

            connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            command = new SqlCeCommand(query, connection);
            connection.Open();
            SqlCeDataReader reader = command.ExecuteReader();

            try
            {
                // Iterate through the results
                while (reader.Read())
                {
                    AccountSums ad = new AccountSums();
                    ad.accountID = reader.GetInt16(0);
                    ad.creditDebit = reader.GetBoolean(1);
                    ad.balance = reader.GetDecimal(2);

                    queryResults.Add(ad);
                }
            }
            finally
            {
                // Always call Close the reader and connection when done reading
                reader.Close();
                connection.Close();
            }

            return queryResults;
        }

        static public List<AccountSums> myGetAccountSumsBeforeDate(DateTime date)
        {
            List<AccountSums> queryResults = new List<AccountSums>();
            SqlCeConnection connection;
            SqlCeCommand command;
            string query;

            query = "  SELECT accountID, creditDebit, SUM(amount) as [sum]";
            query += " FROM LineItem ";
            query += " WHERE date < '" + date.Date.ToString("d") + "' ";
            query += " GROUP BY accountID, creditDebit ";

            connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            command = new SqlCeCommand(query, connection);
            connection.Open();
            SqlCeDataReader reader = command.ExecuteReader();

            try
            {
                // Iterate through the results
                while (reader.Read())
                {
                    AccountSums ad = new AccountSums();
                    ad.accountID = reader.GetInt16(0);
                    ad.creditDebit = reader.GetBoolean(1);
                    ad.balance = reader.GetDecimal(2);

                    queryResults.Add(ad);
                }
            }
            finally
            {
                // Always call Close the reader and connection when done reading
                reader.Close();
                connection.Close();
            }

            return queryResults;
        }
    }

    #endregion Account Sums View Query


    #region Envelope Details View Query

    public class EnvelopeBalanceDetails
    {
        public short envelopeID;
        public bool error;
        public string name;
        public decimal balance;
    }

    partial class FFDBDataSet
    {
        static public List<EnvelopeBalanceDetails> myGetChildEnvelopeBalanceDetails(short parentID)
        {
            List<EnvelopeBalanceDetails> queryResults = new List<EnvelopeBalanceDetails>();
            SqlCeConnection connection;
            SqlCeCommand command;
            string query;

            query = "  SELECT Envelope.id AS envelopeID, Envelope.name, Envelope.currentBalance, Errors.error ";
            query += " FROM Envelope LEFT JOIN ( ";
			query += "      SELECT DISTINCT(SubLineItem.envelopeID), 1 AS error ";
			query += "      FROM SubLineItem INNER JOIN ( ";
			query += "              SELECT id ";
			query += "              FROM LineItem ";
			query += "              WHERE transactionError = 1 OR lineError = 1 ) AS l ";
			query += "          ON SubLineItem.lineItemID = l.id ) AS Errors ";
		    query += "      ON Envelope.id = Errors.envelopeID ";
            query += " WHERE Envelope.closed = 0 AND Envelope.id > 0 AND Envelope.parentEnvelope = " + parentID.ToString();
            query += " ORDER BY Envelope.name";

            connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            command = new SqlCeCommand(query, connection);
            connection.Open();
            SqlCeDataReader reader = command.ExecuteReader();

            try
            {
                // Iterate through the results
                while (reader.Read())
                {
                    EnvelopeBalanceDetails ed = new EnvelopeBalanceDetails();
                    ed.envelopeID = reader.GetInt16(0);
                    ed.name = reader.GetString(1);
                    ed.balance = reader.GetDecimal(2);
                    ed.error = !reader.IsDBNull(3); // Error column is NULL for no error and 1 for error

                    queryResults.Add(ed);
                }
            }
            finally
            {
                // Always call Close the reader and connection when done reading
                reader.Close();
                connection.Close();
            }

            return queryResults;
        }
    }

    #endregion Envelope Details Query


    #region Envelope Sums View Query

    public class EnvelopeSums
    {
        public short envelopeID;
        public bool creditDebit;
        public decimal balance;
    }

    partial class FFDBDataSet
    {
        static public List<EnvelopeSums> myGetEnvelopeSums()
        {
            List<EnvelopeSums> queryResults = new List<EnvelopeSums>();
            SqlCeConnection connection;
            SqlCeCommand command;
            string query;

            query = "  SELECT SubLineItem.envelopeID, LineItem.creditDebit, SUM(SubLineItem.amount) as [sum] ";
            query += " FROM LineItem INNER JOIN SubLineItem ON LineItem.id = SubLineItem.lineItemID ";
            query += " GROUP BY SubLineItem.envelopeID, LineItem.creditDebit ";

            connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            command = new SqlCeCommand(query, connection);
            connection.Open();
            SqlCeDataReader reader = command.ExecuteReader();

            try
            {
                // Iterate through the results
                while (reader.Read())
                {
                    EnvelopeSums ad = new EnvelopeSums();
                    ad.envelopeID = reader.GetInt16(0);
                    ad.creditDebit = reader.GetBoolean(1);
                    ad.balance = reader.GetDecimal(2);

                    queryResults.Add(ad);
                }
            }
            finally
            {
                // Always call Close the reader and connection when done reading
                reader.Close();
                connection.Close();
            }

            return queryResults;
        }

        static public List<EnvelopeSums> myGetEnvelopeSumsBeforeDate(DateTime date)
        {
            List<EnvelopeSums> queryResults = new List<EnvelopeSums>();
            SqlCeConnection connection;
            SqlCeCommand command;
            string query;

            query = "  SELECT SubLineItem.envelopeID, LineItem.creditDebit, SUM(SubLineItem.amount) as [sum] ";
            query += " FROM LineItem INNER JOIN SubLineItem ON LineItem.id = SubLineItem.lineItemID ";
            query += " WHERE LineItem.date < '" + date.Date.ToString("d") + "' ";
            query += " GROUP BY SubLineItem.envelopeID, LineItem.creditDebit ";

            connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            command = new SqlCeCommand(query, connection);
            connection.Open();
            SqlCeDataReader reader = command.ExecuteReader();

            try
            {
                // Iterate through the results
                while (reader.Read())
                {
                    EnvelopeSums ad = new EnvelopeSums();
                    ad.envelopeID = reader.GetInt16(0);
                    ad.creditDebit = reader.GetBoolean(1);
                    ad.balance = reader.GetDecimal(2);

                    queryResults.Add(ad);
                }
            }
            finally
            {
                // Always call Close the reader and connection when done reading
                reader.Close();
                connection.Close();
            }

            return queryResults;
        }
    }

    #endregion Envelope Sums View Query


    #region AEBalancee Sums View Query

    public class AEBalanceSums
    {
        public short accountID;
        public short envelopeID;
        public bool creditDebit;
        public decimal balance;
    }

    partial class FFDBDataSet
    {
        static public List<AEBalanceSums> myGetAEBalanceSums()
        {
            List<AEBalanceSums> queryResults = new List<AEBalanceSums>();
            SqlCeConnection connection;
            SqlCeCommand command;
            string query;

            query = "  SELECT LineItem.accountID, SubLineItem.envelopeID, LineItem.creditDebit, SUM(SubLineItem.amount) as [sum] ";
            query += " FROM LineItem INNER JOIN SubLineItem ON LineItem.id = SubLineItem.lineItemID ";
            query += " GROUP BY LineItem.accountID, SubLineItem.envelopeID, LineItem.creditDebit ";

            connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            command = new SqlCeCommand(query, connection);
            connection.Open();
            SqlCeDataReader reader = command.ExecuteReader();

            try
            {
                // Iterate through the results
                while (reader.Read())
                {
                    AEBalanceSums ad = new AEBalanceSums();
                    ad.accountID = reader.GetInt16(0);
                    ad.envelopeID = reader.GetInt16(1);
                    ad.creditDebit = reader.GetBoolean(2);
                    ad.balance = reader.GetDecimal(3);

                    queryResults.Add(ad);
                }
            }
            finally
            {
                // Always call Close the reader and connection when done reading
                reader.Close();
                connection.Close();
            }

            return queryResults;
        }

        static public List<AEBalanceSums> myGetAEBalanceSumsBeforeDate(DateTime date)
        {
            List<AEBalanceSums> queryResults = new List<AEBalanceSums>();
            SqlCeConnection connection;
            SqlCeCommand command;
            string query;

            query = "  SELECT LineItem.accountID, SubLineItem.envelopeID, LineItem.creditDebit, SUM(SubLineItem.amount) as [sum] ";
            query += " FROM LineItem INNER JOIN SubLineItem ON LineItem.id = SubLineItem.lineItemID ";
            query += " WHERE LineItem.date < '" + date.Date.ToString("d") + "' ";
            query += " GROUP BY LineItem.accountID, SubLineItem.envelopeID, LineItem.creditDebit ";

            connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            command = new SqlCeCommand(query, connection);
            connection.Open();
            SqlCeDataReader reader = command.ExecuteReader();

            try
            {
                // Iterate through the results
                while (reader.Read())
                {
                    AEBalanceSums ad = new AEBalanceSums();
                    ad.accountID = reader.GetInt16(0);
                    ad.envelopeID = reader.GetInt16(1);
                    ad.creditDebit = reader.GetBoolean(2);
                    ad.balance = reader.GetDecimal(3);

                    queryResults.Add(ad);
                }
            }
            finally
            {
                // Always call Close the reader and connection when done reading
                reader.Close();
                connection.Close();
            }

            return queryResults;
        }
    }

    #endregion AEBalancee Sums View Query


    #region Sub Account / Envelope Details View Query

    public class SubBalanceDetails
    {
        public short id;
        public string name;
        public decimal subCurrentBalance;
    }

    partial class FFDBDataSet
    {
        static public List<SubBalanceDetails> myGetSubAccountBalanceDetails(short accountID)
        {
            List<SubBalanceDetails> queryResults = new List<SubBalanceDetails>();
            SqlCeConnection connection;
            SqlCeCommand command;
            string query;

            query = "  SELECT AEBalance.envelopeID, Envelope.fullName, AEBalance.currentBalance ";
            query += " FROM AEBalance INNER JOIN Envelope ON AEBalance.envelopeID = Envelope.id ";
            query += " WHERE AEBalance.currentBalance <> 0.0 AND AEBalance.accountID = " + accountID.ToString();
            query += " ORDER BY Envelope.fullName ";

            connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            command = new SqlCeCommand(query, connection);
            connection.Open();
            SqlCeDataReader reader = command.ExecuteReader();

            try
            {
                // Iterate through the results
                while (reader.Read())
                {
                    SubBalanceDetails ad = new SubBalanceDetails();
                    ad.id = reader.GetInt16(0);
                    ad.name = reader.GetString(1);
                    ad.subCurrentBalance = reader.GetDecimal(2);

                    queryResults.Add(ad);
                }
            }
            finally
            {
                // Always call Close the reader and connection when done reading
                reader.Close();
                connection.Close();
            }

            return queryResults;
        }

        static public List<SubBalanceDetails> myGetSubEnvelopeBalanceDetails(short envelopeID)
        {
            List<SubBalanceDetails> queryResults = new List<SubBalanceDetails>();
            SqlCeConnection connection;
            SqlCeCommand command;
            string query;

            query = "  SELECT AEBalance.accountID, Account.name, AEBalance.currentBalance ";
            query += " FROM AEBalance INNER JOIN Account ON AEBalance.accountID = Account.id ";
            query += " WHERE AEBalance.currentBalance <> 0.0 AND AEBalance.envelopeID = " + envelopeID.ToString();
            query += " ORDER BY Account.name ";

            connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            command = new SqlCeCommand(query, connection);
            connection.Open();
            SqlCeDataReader reader = command.ExecuteReader();

            try
            {
                // Iterate through the results
                while (reader.Read())
                {
                    SubBalanceDetails ad = new SubBalanceDetails();
                    ad.id = reader.GetInt16(0);
                    ad.name = reader.GetString(1);
                    ad.subCurrentBalance = reader.GetDecimal(2);

                    queryResults.Add(ad);
                }
            }
            finally
            {
                // Always call Close the reader and connection when done reading
                reader.Close();
                connection.Close();
            }

            return queryResults;
        }
    }

    #endregion Sub Account Details Query


    #region My Find All Transaction and Line Errors

    partial class FFDBDataSet
    {
        static public void myFindAllErrors()
        {
            SqlCeConnection connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            SqlCeCommand command = new SqlCeCommand();
            string query;
            int result;

            connection.Open();
            command.Connection = connection;

            // Remove all the errors
            query = "UPDATE LineItem SET transactionError = 0, lineError = 0";
            command.CommandText = query;
            result = command.ExecuteNonQuery();

            // Find all the transaction errors
            query = " UPDATE LineItem SET transactionError = 1 WHERE transactionID IN";
            query += "   (SELECT t1.transactionID FROM ";
            query += "      (SELECT SUM(amount) AS [Sum], transactionID FROM LineItem WHERE creditDebit = 0 GROUP BY transactionID) AS t1";
            query += "    INNER JOIN ";
            query += "      (SELECT SUM(amount) AS [Sum], transactionID FROM LineItem WHERE creditDebit = 1 GROUP BY transactionID) AS t2 ";
            query += "    ON t1.transactionID = t2.transactionID ";
            query += "    WHERE t1.Sum <> t2.sum)";
            command.CommandText = query;
            result = command.ExecuteNonQuery();

            // Find all the line errors
            query = " UPDATE LineItem SET lineError = 1 WHERE id IN ";
            query += "   (SELECT Line.id FROM ";
            query += "      (SELECT id, amount FROM LineItem) AS Line ";
            query += "    INNER JOIN ";
            query += "      (SELECT lineItemID, SUM(amount) AS [Sum] FROM SubLineItem GROUP BY lineItemID) AS SubLine ";
            query += "    ON Line.id = SubLine.lineItemID ";
            query += "    WHERE Line.amount <> SubLine.Sum) ";
            command.CommandText = query;
            result = command.ExecuteNonQuery();


            connection.Close();
        }
    }

    #endregion My Find All Errors


    #region My Get Remaining Transaction Details

    public class OtherLineDetails
    {
        public int id;
        public bool creditDebit;
        public short accountID;
        public decimal amount;
    }

    partial class FFDBDataSet
    {
        static public List<OtherLineDetails> myGetOtherLinesInTrans(int lineID, int transID)
        {
            List<OtherLineDetails> queryResults = new List<OtherLineDetails>();
            SqlCeConnection connection;
            SqlCeCommand command;
            string query;

            // Get the details of the other line in the transaction
            query = " SELECT id, accountID, creditDebit, amount";
            query += " FROM LineItem ";
            query += " WHERE transactionID = " + transID.ToString() + " AND id <> " + lineID.ToString();

            connection = new SqlCeConnection(Properties.Settings.Default.FFDBConnectionString);
            command = new SqlCeCommand(query, connection);
            connection.Open();
            SqlCeDataReader reader = command.ExecuteReader();

            try
            {
                // Iterate through the results
                while (reader.Read())
                {
                    OtherLineDetails ad = new OtherLineDetails();
                    ad.id = reader.GetInt32(0);
                    ad.accountID = reader.GetInt16(1);
                    ad.creditDebit = reader.GetBoolean(2);
                    ad.amount = reader.GetDecimal(3);

                    queryResults.Add(ad);
                }
            }
            finally
            {
                // Always call Close the reader and connection when done reading
                reader.Close();
                connection.Close();
            }

            return queryResults;



        }
    }

    #endregion My Get Remaining Transaction Details


} // END namespace FamilyFinance


