using System;
using System.Windows.Forms;
using System.Configuration;
using System.Runtime.Remoting.Channels;
using L3.Cargo.Communications.CargoHost;
using L3.Cargo.Communications.EventsLogger.Client;
using l3.cargo.corba;



/// <summary>
/// Summary description for CargoHostInterface.
/// This class implements core functionality to support interfacing with CargoHost module using
/// Corba architecture.
/// It acquires XrayHost and Host object of CargoHost module and implements following helper functions
/// required in this interface
/// 1. string CreateCase(string containerid)
/// 2. bool AddOCRFile(string caseid, string fileName)
/// 3. string SnmStartScan()
/// 4. bool AddSNMFile(string fileName)
/// 5. bool SnmStopScan()
/// 
/// 
/// </summary>
public class CargoHostInterface 
{
	
    private CargoHostEndPoint               _cargoHostEndPoint;
    private EventLoggerAccess               _logger;

	/// <summary>
	/// CargoHostInterface.  This is the constructor for this
	/// class. It publishes XI and SNM objects for CargoHost to receive notifications
	/// 
	///	Arguments:
	///	
	///	
	///	
	///		none
	///	Exceptions:
	///		none
	///	Return:
	///		none
	/// </summary>
	public CargoHostInterface(EventLoggerAccess logger)
	{

		try
		{
            string CORBA_NS_Host = (string)System.Configuration.ConfigurationManager.AppSettings["host"];
            Int32 CORBA_NS_Port = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["port"]);

            _cargoHostEndPoint = new CargoHostEndPoint(CORBA_NS_Host, CORBA_NS_Port);

		}
		catch (Exception e)
		{
			MessageBox.Show(e.StackTrace);
			return;
		}

        _cargoHostEndPoint.Open();

        _logger = logger;
    }
		
	/// <summary>
	/// CreateCase.  This interface function Creates new Case using container id
	/// 
	///	Arguments:
	///		containerid: The container id
	///	Exceptions:
	///		none
	///	Return:
	///		Caseid
	/// </summary>
	public string CreateCase(string containerid)
	{
        string caseid = null;

        try
		{
            //m_XRayHost = GetXrayHost();
            caseid = _cargoHostEndPoint.CreateInitAreaCase();
            //caseid = m_XRayHost.makeCase(containerid);
             XCase currentCase = _cargoHostEndPoint.GetCase(caseid);
             currentCase.setContainerId(containerid);
		}
		catch (Exception e1)
		{
			_logger.LogError("OM - " + e1.Message);
		}

        return caseid;
	}

	

	/// <summary>
	/// CreateCase.  This interface function Creates new Case and returns CaseId
	/// 
	///	Arguments:
	///		void
	///	Exceptions:
	///		none
	///	Return:
	///		Caseid
	/// </summary>
	public string CreateCase()
	{
        string caseid = null;

        try
        {
            //m_XRayHost = GetXrayHost();
            caseid = _cargoHostEndPoint.CreateInitAreaCase();
            //caseid = m_XRayHost.makeCase(containerid);
            XCase currentCase = _cargoHostEndPoint.GetCase(caseid);
        }
        catch (Exception e1)
        {
            _logger.LogError("OM - " + e1.Message);
        }
        
        return caseid;
    }


    /// <summary>
    /// GetScanCaseList.  This interface function returns list of Cases in the Scan Queue
    /// 
    ///	Arguments:
    ///		void
    ///	Exceptions:
    ///		none
    ///	Return:
    ///		Caseid list
    /// </summary>
    public string[] GetScanCaseList ()
    {

        try
        {
            string[] caseList;
            caseList = _cargoHostEndPoint.GetScanAreaCases();
            return caseList;
        }
        catch (Exception e1)
        {
            _logger.LogError("OM - " + e1.Message);
        }

        return null;
    }

    
    /// <summary>
    /// SetContainerNumber.  This interface function Creates new Case and returns CaseId
    /// 
    ///	Arguments:
    ///		void
    ///	Exceptions:
    ///		none
    ///	Return:
    ///		Caseid
    /// </summary>
    public bool SetContainerNumber(string caseId, string containerCode)
    {
        try
        {
            XCase currentCase = _cargoHostEndPoint.GetCase(caseId);
            currentCase.setContainerCode(containerCode);
            return true;
        }
        catch (Exception e1)
        {
            _logger.LogError("OM - " + e1.Message);
        }

        return false;
    }


	/// <summary>
	/// SetVehicleRegistrationNumber.  This interface function Creates new Case and returns CaseId
	/// 
	///	Arguments:
	///		void
	///	Exceptions:
	///		none
	///	Return:
	///		Caseid
	/// </summary>
	public bool SetVehicleRegistrationNumber(string caseId, string regNumber)
	{
        try
        {
            XCase currentCase = _cargoHostEndPoint.GetCase(caseId);
            currentCase.setConveyanceId(regNumber);
            return true;
        }
        catch (Exception e1)
        {
            _logger.LogError("OM - " + e1.Message);
        }

        return false;
    }

	/// <summary>
	/// AddOCRFile.  This interface function adds OCR file into case folder 
	/// 
	///	Arguments:
	///		caseid: Case id of current live case
	///		fileName: Absolute path of file to be added in case folder
	///	Exceptions:
	///		none
	///	Return:
	///		bool
	/// </summary>
	public bool AddOCRFile(string caseid, string fileName)
	{
        try
        {
            XCase currentCase = _cargoHostEndPoint.GetCase(caseid);

            Attachment att = new Attachment(fileName, "OCR", "SYSTEM", DateTime.Now.ToString());

            currentCase.addAttachment(att);
            return true;
        }
        catch (Exception e1)
        {
            _logger.LogError("OM - " + e1.Message);
        }

        return false;
    }

    /// <summary>
    /// AddManifest.  This interface function adds Manifest into case folder 
    /// 
    ///	Arguments:
    ///		manid: Manifest id of current live case
    ///		caseid: Case id of current live case
    ///	Exceptions:
    ///		none
    ///	Return:
    ///		bool
    /// </summary>
    public bool AddManifest(string manid, string caseid)
    {
        //try
        //{
        //    m_Host = GetHost();
        //    m_Host.addManifest(manid, caseid);
        //    bRet = true;
        //}
        //catch (CargoException e1)
        //{
        //    _logger.LogError("OM - " + e1.error_msg);
        //}
        return false;
    }

	public void LinkCases(string rootCaseId, string childCaseId)
	{
        try
        {

            XCase currentCase = _cargoHostEndPoint.GetCase(rootCaseId);

            currentCase.setLinkedCase(childCaseId);
        }
        catch (Exception e1)
        {
            _logger.LogError("OM - " + e1.Message);
        }
	}
}

