using System;
using System.Windows.Forms;
using System.Configuration;
using System.Runtime.Remoting.Channels;

using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;

using omg.org.CosNaming;
using omg.org.CORBA;

using l3.cargo.corba;
using l3.cargo.korea.corba;



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
    static IiopChannel                      m_Channel;

    static Host                             m_Host;
    static XRayHost                         m_XRayHost;
    static Logger                           m_CargoHostLogger;


    static NamingContext                    m_NameService;
    private NameComponent[]                 m_ncXrayHost = new NameComponent[]{ new NameComponent("cargo", "context"),
                                                                                new NameComponent("korea", "context"),
                                                                                new NameComponent("XRayHost", "object")};

    private NameComponent[]                 m_ncHost = new NameComponent[]{ new NameComponent("cargo", "context"),
                                                                                new NameComponent("host", "object")};

    private NameComponent[]                 m_ncXI = new NameComponent[]{ new NameComponent("cargo", "context"),
                                                                             new NameComponent("xi", "object")};

    private NameComponent[]                 m_ncLogger = new NameComponent[]{ new NameComponent("cargo", "context"),
                                                                                new NameComponent("hostLogger", "object")};

    private XI_Impl                         xrayInterface = null;

    /// <summary>
    /// CargoHostInterface.  This is the constructor for this
    /// class. It publishes XI and SNM objects for CargoHost to receive notifications
    ///
    /// Arguments:
    ///
    ///
    ///
    ///     none
    /// Exceptions:
    ///     none
    /// Return:
    ///     none
    /// </summary>
    public CargoHostInterface()
    {
        try
        {
            // register the channel
            m_Channel = new IiopChannel(0); // assign port automatically
            ChannelServices.RegisterChannel(m_Channel, false);

            CorbaInit   m_Init = CorbaInit.GetInit();

            string CORBA_NS_Host = (string)ConfigurationManager.AppSettings["host"];
            Int32 CORBA_NS_Port = Int32.Parse(ConfigurationManager.AppSettings["port"]);

            m_NameService = m_Init.GetNameService(CORBA_NS_Host, (int)CORBA_NS_Port);

            xrayInterface = new XI_Impl();
            NameComponent[] ncXI = GetNameComponent(xrayInterface, "xi");
        }
        catch (Exception e)
        {
            MessageBox.Show(e.StackTrace);
            return;
        }

        // Get Logger from CargoHost
        try
        {
            m_CargoHostLogger = (l3.cargo.corba.Logger)m_NameService.resolve(m_ncLogger);
        }
        catch (CargoException e1)
        {
            MessageBox.Show(e1.error_msg);
        }
        catch ( omg.org.CORBA.AbstractCORBASystemException a)
        {
            MessageBox.Show(a.Message);
        }
    }

    protected NameComponent[] GetNameComponent(System.Object obj, string objectName)
    {
        NameComponent[] nc = new
            NameComponent[] { new NameComponent("cargo", "context"), new NameComponent(objectName, "object") };

        try
        {
            if (obj.GetType().FullName.Equals("XI_Impl"))
            {
                m_NameService.bind(nc, (XI_Impl)obj);
            }

        }
        catch (AbstractCORBASystemException ex)
        {
            MessageBox.Show("Unable to find XI Object\n" + ex.Message);
            return null;
        }
        catch (System.Exception e)
        {
            string str = e.Message;

            try
            {
                if (obj.GetType().FullName.Equals("XI_Impl"))
                {
                    m_NameService.rebind(nc, (XI_Impl)obj);
                }
            }
            catch (System.Exception e1)
            {
                MessageBox.Show("Rebinding XI Failed\n" + e1.Message);
            }
        }

        return nc;
    }

    /// <summary>
    /// Cleanup.  This helper function for cleaning up before exitting
    ///
    /// Arguments:
    ///     none
    /// Exceptions:
    ///     none
    /// Return:
    ///
    /// </summary>
    public void CleanUp()
    {
//      m_NameService.unbind(m_ncSNM);
//      m_NameService.unbind(ncXI);
        m_NameService.unbind(m_ncLogger);

        m_NameService.destroy();
    }

    /// <summary>
    /// GetXrayHost.  This helper function gets XRayHost object from CargoHost module
    ///
    /// Arguments:
    ///     none
    /// Exceptions:
    ///     none
    /// Return:
    ///     XRayHost
    /// </summary>
    private XRayHost GetXrayHost()
    {
        try
        {
            m_XRayHost = (XRayHost)m_NameService.resolve(m_ncXrayHost);
        }
        catch (CargoException e1)
        {
//          MessageBox.Show(e1.error_msg);
            m_CargoHostLogger.logError("OM - " + e1.error_msg);
        }
        catch ( omg.org.CORBA.AbstractCORBASystemException a)
        {
//          MessageBox.Show(a.ToString());
            m_CargoHostLogger.logError("OM - " + a.ToString());
        }

        return m_XRayHost;
    }

    /// <summary>
    /// GetXrayHost.  This helper function gets XRayHost object from CargoHost module
    ///
    /// Arguments:
    ///     none
    /// Exceptions:
    ///     none
    /// Return:
    ///     XRayHost
    /// </summary>
    public Logger GetLogger()
    {
        try
        {
            m_CargoHostLogger = (Logger)m_NameService.resolve(m_ncLogger);
        }
        catch (CargoException e1)
        {
//          MessageBox.Show(e1.error_msg);
            m_CargoHostLogger.logError("OM - " + e1.error_msg);
        }
        catch ( omg.org.CORBA.AbstractCORBASystemException a)
        {
//          MessageBox.Show(a.ToString());
            m_CargoHostLogger.logError("OM - " + a.ToString());
        }
        return m_CargoHostLogger;
    }

    /// <summary>
    /// GetHost.  This helper function gets Host object from CargoHost module
    ///
    /// Arguments:
    ///     none
    /// Exceptions:
    ///     none
    /// Return:
    ///     Host
    /// </summary>
    private Host GetHost()
    {
        try
        {
            m_Host = (Host)m_NameService.resolve(m_ncHost);
        }
        catch (CargoException e1)
        {
//          MessageBox.Show(e1.error_msg);
            m_CargoHostLogger.logError("OM - " + e1.error_msg);
        }
        catch ( omg.org.CORBA.AbstractCORBASystemException a)
        {
//          MessageBox.Show(a.ToString());
            m_CargoHostLogger.logError("OM - " + a.ToString());
        }
        return m_Host;
    }

    /// <summary>
    /// CreateCase.  This interface function Creates new Case using container id
    ///
    /// Arguments:
    ///     containerid: The container id
    /// Exceptions:
    ///     none
    /// Return:
    ///     Caseid
    /// </summary>
    public string CreateCase(string containerid)
    {
        string caseid = null;

        try
        {
            m_XRayHost = GetXrayHost();
            caseid = m_XRayHost.makeCase(containerid);
        }
        catch (CargoException e1)
        {
//          MessageBox.Show(e1.error_msg);
            m_CargoHostLogger.logError("OM - " + e1.error_msg);
        }
        return caseid;

    }

    /// <summary>
    /// CreateCase.  This interface function Creates new Case and returns CaseId
    ///
    /// Arguments:
    ///     void
    /// Exceptions:
    ///     none
    /// Return:
    ///     Caseid
    /// </summary>
    public string CreateCase()
    {
        string caseid = null;

        try
        {
            Host h = GetHost();

            CaseManager cm = h.getCaseManager();

            caseid = cm.makeCase();
        }
        catch (CargoException e1)
        {
//          MessageBox.Show(e1.error_msg);
            m_CargoHostLogger.logError("OM - " + e1.error_msg);
        }
        return caseid;
    }

    /// <summary>
    /// SetVehicleRegistrationNumber.  This interface function Creates new Case and returns CaseId
    ///
    /// Arguments:
    ///     void
    /// Exceptions:
    ///     none
    /// Return:
    ///     Caseid
    /// </summary>
    public string SetVehicleRegistrationNumber(string caseId, string regNumber)
    {
        string caseid = null;

        try
        {
            Host h = this.GetHost();

            CaseManager cm = h.getCaseManager();

            cm.setVehicleRegistrationNumber(caseId, regNumber);
        }
        catch (CargoException e1)
        {
//          MessageBox.Show(e1.error_msg);
            m_CargoHostLogger.logError("OM - " + e1.error_msg);
        }
        return caseid;
    }

    /// <summary>
    /// AddOCRFile.  This interface function adds OCR file into case folder
    ///
    /// Arguments:
    ///     caseid: Case id of current live case
    ///     fileName: Absolute path of file to be added in case folder
    /// Exceptions:
    ///     none
    /// Return:
    ///     bool
    /// </summary>
    public bool AddOCRFile(string caseid, string fileName)
    {
        bool bRet = false;
        string filetype = "OCR";
        try
        {
            m_Host = GetHost();
            CaseManager cm = m_Host.getCaseManager();
            XCase liveCase = cm.getLiveCase(caseid);
            liveCase.addGeneralFile(fileName, filetype);
            bRet = true;
        }
        catch(CargoException e1)
        {
            m_CargoHostLogger.logError("OM - " + e1.error_msg);
//          MessageBox.Show(e1.error_msg);
        }
        return bRet;

    }

    /// <summary>
    /// AddManifest.  This interface function adds Manifest into case folder
    ///
    /// Arguments:
    ///     manid: Manifest id of current live case
    ///     caseid: Case id of current live case
    /// Exceptions:
    ///     none
    /// Return:
    ///     bool
    /// </summary>
    public bool AddManifest(string manid, string caseid)
    {
        bool bRet = false;
        try
        {
            m_Host = GetHost();
            m_Host.addManifest(manid, caseid);
            bRet = true;
        }
        catch (CargoException e1)
        {
            m_CargoHostLogger.logError("OM - " + e1.error_msg);
        }
        return bRet;

    }

    public void LinkCases(string rootCaseId, string childCaseId)
    {
        try
        {
            m_Host = GetHost();
            CaseManager cm = m_Host.getCaseManager();
            XCase liveCase = cm.getLiveCase(rootCaseId);
            liveCase.setLinkedCase(childCaseId);
        }
        catch(CargoException e1)
        {
//          MessageBox.Show(e1.error_msg);
            m_CargoHostLogger.logError("OM - " + e1.error_msg);
        }
    }
}
