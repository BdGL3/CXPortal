using System;

namespace L3.Cargo.Common
{
    public class ErrorMessages
    {

        // The error strings have been replaced with attributes that get the string from the resource files.
        // The original strings have been left but commented out for reference.

        //public const string CASE_ID_INVALID = @"The case id is empty, null, or invalid.";
        public static string CASE_ID_INVALID
        {
            get { return Resources.Error_CaseIdInvalid; }
        }

        //public const string CASE_CURRENTLY_IN_USE = @"The case requested is currently in use.";
        public static string CASE_CURRENTLY_IN_USE
        {
            get { return Resources.Error_CaseCurrentlyInUse; }
        }

        //public const string CASE_NOT_ACCESSIBLE = @"The Workstation does not have access to this case.";
        public static string CASE_NOT_ACCESSIBLE
        {
            get { return Resources.Error_CaseNotAccessible; }
        }

        //public const string NO_LIVE_CASE = @"There are currently no available pending cases.";
        public static string NO_LIVE_CASE
        {
            get { return Resources.Error_NoLiveCase; }
        }

        //public const string LOAD_BALANCE_DELAY_CASE_REQUEST = @"Case Request not processed for load balance reasons.";
        public static string LOAD_BALANCE_DELAY_CASE_REQUEST
        {
            get { return Resources.Error_LoadBalanceDelayCaseRequirements; }
        }

        //public const string CASE_VERSION_MISMATCH = @"XML file version does not match schema.";
        public static string CASE_VERSION_MISMATCH
        {
            get { return Resources.Error_CaseVersionMismatch; }
        }

        //public const string FILENAME_EMPTY = @"The filename length is 0, please provide a valid filename";
        public static string FILENAME_EMPTY
        {
            get { return Resources.Error_FilenameEmpty; }
        }

        //public const string THUMBNAIL_CREATE_FAIL = @"The Dll was unable to create a thumbnail from the PXE file passed.";
        public static string THUMBNAIL_CREATE_FAIL
        {
            get { return Resources.Error_ThumbnailCreateFail; }
        }

        //public const string INVALID_FUNCTION = @"The function being called is invalid or has not been implemented.";
        public static string INVALID_FUNCTION
        {
            get { return Resources.Error_InvalidFunction; }
        }

        //public const string INVALID_LOGIN = @"Username and password are incorrect.  Please contact your system administrator.";
        public static string INVALID_LOGIN
        {
            get { return Resources.Error_InvalidLogin; }
        }

        //public const string NO_USER_PROFILE = @"No user profile could be found.  Please contact your system administrator.";
        public static string NO_USER_PROFILE
        {
            get { return Resources.Error_NoUserProfile; }
        }

        //public const string NO_LIVE_SOURCES = @"There are currently no available pending case sources";
        public static string NO_LIVE_SOURCES
        {
            get { return Resources.Error_NoLiveSources; }
        }

        //public const string NO_ARCHIVE_SOURCES = @"There are currently no available archive case sources";
        public static string NO_ARCHIVE_SOURCES
        {
            get { return Resources.Error_NoArchiveSources; }
        }

        //public const string CASE_LIST_NOT_AVAILABLE = @"The list of available cases is currently not available.";
        public static string CASE_LIST_NOT_AVAILABLE
        {
            get { return Resources.Error_CaseListNotAvailable; }
        }

        //public const string SOURCE_NOT_AVAILABLE = @"The case source is no longer available.";
        public static string SOURCE_NOT_AVAILABLE
        {
            get { return Resources.Error_SourceNotAvailable; }
        }

        //public const string SOURCE_TYPE_UNKNOWN = @"The select case source Type is unknown.";
        public static string SOURCE_TYPE_UNKNOWN
        {
            get { return Resources.Error_SourceTypeUnknown; }
        }

        //public const string ONE_PENDING_CASE_ONLY = @"The pending case is currently being shown and must be cleared before a new one can be selected";
        public static string ONE_PENDING_CASE_ONLY
        {
            get { return Resources.Error_OnePendingCaseOnly; }
        }

        //public const string CASE_DOES_NOT_EXIST = @"Specified Case does not exist in the Case List.";
        public static string CASE_DOES_NOT_EXIST
        {
            get { return Resources.Error_CaseDoesNotExist; }
        }

        //public const string CASE_NOT_LISTED = @" Case is not listed in the case list: ";
        public static string CASE_NOT_LISTED
        {
            get { return Resources.Error_CaseNotListed; }
        }
    }
}