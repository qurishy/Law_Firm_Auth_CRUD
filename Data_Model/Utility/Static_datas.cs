﻿namespace Law_Model.Static_file
{

    public static class Static_datas
    {
        public enum UserRole
        {
            Admin,
            Lawyer,
            Client
        }

        public enum CaseType
        {
            Criminal,
            Civil,
            Family,
            Corporate,
            RealEstate,
            Intellectual,
            Immigration,
            Other
        }
        public enum CaseStatus
        {
            New,
            InProgress,
            OnHold,
            Closed,
            Archived
        }
    }
}
