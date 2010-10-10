﻿
namespace FamilyFinance.Database
{

    /////////////////////////////////
    // Constants
    public class LineCD
    {
        public const bool CREDIT = false;
        public const bool DEBIT = true;
    }

    public class LineState
    {
        public const string CLEARED = "C";
        public const string RECONSILED = "R";
        public const string PENDING = " ";
    }

    public class SpclLineType
    {
        public const int NULL = -1;
    }

    public class SpclAccount
    {
        public const int MULTIPLE = -2;
        public const int NULL = -1;
    }

    public class SpclAccountCat
    {
        public const byte INCOME = 1;
        public const byte ACCOUNT = 2;
        public const byte EXPENSE = 3;
    }

    public class SpclAccountType
    {
        public const int NULL = -1;
    }

    public class SpclEnvelope
    {
        public const int SPLIT = -2;
        public const int NULL = -1;
        public const int NOENVELOPE = 0;
    }

    public class SpclEnvelopeGroup
    {
        public const int NULL = -1;
    }


    public class SpclBank
    {
        public const int NULL = -1;
    }

}
