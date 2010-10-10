﻿using FamilyFinance.Database;

namespace FamilyFinance.Model
{
    /// <summary>
    /// Models the account elements for reading and changing values.
    /// </summary>
    class AccountModel : ModelBase
    {
        /// <summary>
        /// Local referance to the account row this object is modeling.
        /// </summary>
        protected FFDataSet.AccountRow accountRow;

        /// <summary>
        /// Gets the ID of the account.
        /// </summary>
        public int ID
        {
            get
            {
                return this.accountRow.id;
            }
        }

        /// <summary>
        /// Gets or sets the name of the account.
        /// </summary>
        public string Name 
        {
            get 
            {
                return this.accountRow.name;
            }

            set
            {
                checkRowState();

                this.accountRow.name = value;
                MyData.getInstance().saveAccountRow(this.accountRow);
                this.RaisePropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the typeID of this account.
        /// </summary>
        public int TypeID
        {
            get 
            { 
                return this.accountRow.typeID; 
            }

            set
            {
                checkRowState();

                this.accountRow.typeID = value;
                MyData.getInstance().saveAccountRow(this.accountRow);
                this.RaisePropertyChanged("TypeID");
                this.RaisePropertyChanged("TypeName");
            }
        }

        /// <summary>
        /// Gets the type name of this account.
        /// </summary>
        public string TypeName
        {
            get
            {
                return this.accountRow.AccountTypeRow.name;
            }
        }

        /// <summary>
        /// Gets or sets the Catagory of this account.
        /// </summary>
        public byte CatagoryID
        {
            get
            {
                return this.accountRow.catagory;
            }

            set
            {
                checkRowState();

                this.accountRow.catagory = value;
                MyData.getInstance().saveAccountRow(this.accountRow);
                this.RaisePropertyChanged("CatagoryID");
                this.RaisePropertyChanged("CatagoryName");
            }
        }

        /// <summary>
        /// Gets the gatagory name forthis account.
        /// </summary>
        public string CatagoryName
        {
            get
            {
                return CatagoryModel.getName(this.accountRow.catagory);
            }
        }

        /// <summary>
        /// Gets or sets the Closed flag for this account. True if the account is closed, 
        /// false if the account is open.
        /// </summary>
        public bool Closed
        {
            get
            {
                return this.accountRow.closed;
            }

            set
            {
                checkRowState();

                this.accountRow.closed = value;
                MyData.getInstance().saveAccountRow(this.accountRow);
                this.RaisePropertyChanged("Closed");
            }
        }

        /// <summary>
        /// Gets or sets the flag stating whether or not this account uses envelopes.
        /// </summary>
        public bool UsesEnvelopes
        {
            get
            {
                return this.accountRow.envelopes;
            }

            set
            {
                checkRowState();

                this.accountRow.envelopes = value;
                MyData.getInstance().saveAccountRow(this.accountRow);
                this.RaisePropertyChanged("UsesEnvelopes");
            }
        }

        /// <summary>
        /// Creates the object and keeps a local referance to the given account row.
        /// </summary>
        /// <param name="aRow"></param>
        public AccountModel(FFDataSet.AccountRow aRow)
        {
            this.accountRow = aRow;
        }

        /// <summary>
        /// Creates the object with a referance to a new account row.
        /// </summary>
        public AccountModel()
        {
            this.accountRow = MyData.getInstance().Account.NewAccountRow();
        }

        /// <summary>
        /// Checks the state of the row. If it is a new row (detached) it will be added to the table.
        /// </summary>
        private void checkRowState()
        {
            if (this.accountRow.RowState == System.Data.DataRowState.Detached)
                MyData.getInstance().Account.AddAccountRow(this.accountRow);
        }
   
    }
}
