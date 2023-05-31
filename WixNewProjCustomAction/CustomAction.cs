using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using System.Linq;
using Newtonsoft.Json;

namespace WixNewProjCustomAction
{
    public class CustomActions
    {
        private static List<string> removedFeaturesList = new List<string>();
        [CustomAction]
        public static ActionResult GetProperties(Session session)
        {
            Trace.WriteLine("GetProperties : ");


            string[] a = { "WebExchange1_13", "WebExchange1_14" };

            foreach (string s in a)
            {
                string propertyData = session[s];
                Trace.WriteLine("Prop : " + s);

                CustomActionData iniAddLineData = new CustomActionData(propertyData);
                string section = iniAddLineData["SECTION"];
                string key = iniAddLineData["KEY"];
                string newValue = iniAddLineData["VALUE"];
                if (iniAddLineData.ContainsKey("COMMENT"))
                {
                    string comment = iniAddLineData["COMMENT"];
                    Trace.WriteLine("comment : " + comment);
                }

                Trace.WriteLine("Section : " + section);
                Trace.WriteLine("key : " + key);
                Trace.WriteLine("newValue : " + newValue);
            }
            return ActionResult.Success;
        }
        [CustomAction]
        public static ActionResult GetRemovedFeatures(Session session)
        {
            try
            {
                string previousAddLocal = session["INSTALLEDFEATURES"];
                Trace.WriteLine("In GetRemoveFeature");
                List<string> allFeatureList = new List<string> { "Feature1", "Feature2", "Feature3" };
                foreach (string str in allFeatureList)
                {
                    Trace.WriteLine(str + " : " + session.Features[str].RequestState.ToString());
                }
                if (session["Installed"] != "")
                {
                    foreach (string feature in allFeatureList)
                    {
                        string featureProp = string.Concat(feature, "_LOCAL").ToUpper();
                        session[featureProp] = string.Empty;
                        Trace.WriteLine("Feature Local : " + featureProp);
                        if (session.Features[feature].RequestState == InstallState.Local && previousAddLocal.Contains(feature))
                        {
                            Trace.WriteLine("Feature set");
                            session[featureProp] = "Installed";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.InnerException);
            }
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult SetSelectedFeaturesRegistry(Session session)
        {
            string addLocal = session["ADDLOCAL"];
            string previousAddLocal = session["INSTALLEDFEATURES"];

            string removeLocal = session["REMOVE"];
            string[] removeLocalArray = removeLocal.Split(',');

            string totalAddLocals = string.Concat(addLocal, ",", previousAddLocal);
            string[] totalAddLocalsArray = totalAddLocals.Split(',');
            string[] distinctAllLocals = totalAddLocalsArray.Distinct<string>().ToArray();

            string[] finalAddLocals = distinctAllLocals.Except<string>(removeLocalArray).ToArray<string>();

            string finalFeatures = string.Join(",", finalAddLocals);

            Trace.WriteLine("removeLocal : " + session["REMOVE"]);
            Trace.WriteLine("addLocal : " + session["ADDLOCAL"]);
            Trace.WriteLine("previousAddLocal : " + session["INSTALLEDFEATURES"]);
            Trace.WriteLine("finalFeatures : " + finalFeatures);

            string productCode = session["ProductCode"];
            RegistryKey key = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{productCode}", true);
            key.SetValue("ADDLOCAL", finalFeatures);
            key.Close();

            Trace.WriteLine("Add Locals : " + session["ADDLOCAL"]);
            return ActionResult.Success;
        }




        [CustomAction]
        public static ActionResult AddLocalList(Session session)
        {
            //Trace.WriteLine("Selected Feature : " + session["MsiSelectionTreeSelectedFeature"]);
            //Trace.WriteLine("Feature Action : " + session["MsiSelectionTreeSelectedAction"]);
            //Trace.WriteLine("Custom Feature Action : " + session["FEATUREACTION"]);
            Trace.WriteLine("Add Locals : " + session["AddLocal"]);

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult GetFeatureInfo(Session session)
        {
            List<string> allFeatureList = new List<string> { "Feature1", "Feature2", "Feature3" };

            Trace.WriteLine("Add Locals : " + session["ADDLOCAL"]);
            foreach (string featureName in allFeatureList)
            {
                //Trace.WriteLine(featureName);
                //Trace.WriteLine("CurrentState : " + session.Features[featureName].CurrentState);
                ////Trace.WriteLine("Attributes : " + session.Features[featureName].Attributes.ToString());
                //Trace.WriteLine("Description : " + session.Features[featureName].Description);
                //Trace.WriteLine("Name : " + session.Features[featureName].Name);
                //Trace.WriteLine("RequestState : " + session.Features[featureName].RequestState);
                //Trace.WriteLine("Title : " + session.Features[featureName].Title);
                ////Trace.WriteLine("ValidStates : " + session.Features[featureName].ValidStates.ToString());
            }
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult GetFeaturePath(Session session)
        {
            //Trace.WriteLine("Selected Feature : " + session["MsiSelectionTreeSelectedFeature"]);
            //Trace.WriteLine("Feature Action : " + session["MsiSelectionTreeSelectedAction"]);
            //Trace.WriteLine("Custom Feature Action : " + session["FEATUREACTION"]);
            Trace.WriteLine("Feature Path : " + session["MsiSelectionTreeSelectedPath"]);

            session["FEATUREINSTALLEDLOCATION"] = session["MsiSelectionTreeSelectedPath"];
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult GetSelectedFeatureList(Session session)
        {
            List<string> allFeatureList = new List<string> { "Feature1", "Feature2", "Feature3" };

            List<string> selectedFeatureList = new List<string>();

            foreach (string featureName in allFeatureList)
            {
                if (session.Features[featureName].RequestState == InstallState.Absent)
                {
                    Trace.WriteLine("Absent Selected Feature : " + featureName);
                    selectedFeatureList.Add(featureName);
                }
                else
                {

                    Trace.WriteLine(" Absent Feature Not selected: " + featureName);
                }

            }
            //Trace.WriteLine("Selected Feature : " + session["MsiSelectionTreeSelectedFeature"]);
            //Trace.WriteLine("Feature Action : " + session["MsiSelectionTreeSelectedAction"]);
            //Trace.WriteLine("Custom Feature Action : " + session["FEATUREACTION"]);
            //Trace.WriteLine("Feature Path : " + session["MsiSelectionTreeSelectedPath"]);

            return ActionResult.Success;
        }


        [CustomAction]
        public static ActionResult GetSpokWebBundleUninstallString(Session session)
        {
            Trace.WriteLine("In GetSpokWebBundleUninstallString");
            var upgradeCode = session["SpokWebBundleUpgradeCode"];
            var uninstallProperty = session["SpokWebBundleUninstallProperty"];
            ReadReagistryForUninstallString(session, upgradeCode, uninstallProperty);
            Trace.WriteLine("Out GetSpokWebBundleUninstallString");

            return ActionResult.Success;
        }


        /// <summary>
        /// Common function to retrieve bundle upgrade code
        /// </summary>
        /// <param name="session">MSI Session</param>
        /// <param name="upgradeCode">Bundle Upgrade code for the Wix Bundle</param>
        /// <param name="uninstallProperty">Property to set in the installer. The value will be Quiet Uninstallation String</param>
        private static void ReadReagistryForUninstallString(Session session, string upgradeCode, string uninstallProperty)
        {
            var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            var uninstallRoot = root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            var myPack = uninstallRoot.GetSubKeyNames();
            var quietUninstallString = string.Empty;
            foreach (var item in myPack)
            {
                var sub = uninstallRoot.OpenSubKey(item);
                if (sub.GetValueNames().Contains("BundleUpgradeCode")
                    && sub.GetValue("BundleUpgradeCode") is string[]
                    && ((string[])sub.GetValue("BundleUpgradeCode"))[0] == upgradeCode)
                {
                    quietUninstallString = sub.GetValue("QuietUninstallString", string.Empty, RegistryValueOptions.None) as string;
                    //session[uninstallProperty] = quietUninstallString;
                    session[uninstallProperty] = "MsiExec.exe /X{57838140-7C4D-4DD8-9E98-FAB4C2866B4E} /qn /norestart";
                    Trace.WriteLine("quietUninstallString " + quietUninstallString);

                    break;
                }
            }
        }

        [CustomAction]
        public static ActionResult UpdateEventApiListenerConfig(Session session)
        {
            try
            {
                Trace.WriteLine("In UpdateEventApiListenerConfig");
                string eventApiDir = session.CustomActionData["CONNECTIONSTRING_DBSOURCE"];
                //string eventApiDir = session.CustomActionData.ToString();

                Trace.WriteLine("Connection String : ", eventApiDir);
            }
            catch (Exception ex)
            {
                session.Log("Exception in UpdateEventApiListenerConfig", ex.Message);
                Trace.WriteLine("Exception UpdateEventApiListenerConfig", ex.Message);
            }
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult UpdateConnection(Session session)
        {
            try
            {
                Trace.WriteLine("In UpdateConnection : ", session["AUTOEXPORTTIME"]);
                session["CONNECTIONSTRING"] = session["AUTOEXPORTTIME"];
            }
            catch (Exception ex)
            {
                session.Log("Exception in UpdateEventApiListenerConfig", ex.Message);
                Trace.WriteLine("Exception UpdateEventApiListenerConfig", ex.Message);
            }
            return ActionResult.Success;
        }
    }
}
