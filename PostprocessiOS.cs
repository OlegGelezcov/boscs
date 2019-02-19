using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;
using System.IO;
using System.Linq;
using System.Xml.Linq;
#if UNITY_IOS
 using UnityEditor.iOS.Xcode;   



public class PostprocessiOS {

    private static readonly string locationUsageDescription = "Using the location is necessary for the proper operation of advertising services.";
    private static readonly string kNSLocationAlwaysUsageDescription = "NSLocationAlwaysUsageDescription";
    private static readonly string kNSLocationWhenInUseUsageDescription = "NSLocationWhenInUseUsageDescription";

    [PostProcessBuild(100)]
    public static void AddMissingToXCodeForBos2(BuildTarget target, string pathToBuildProject) {
        #if UNITY_IOS
        AddWatchConnectivityFramework(pathToBuildProject);
        AddCalendarUsageDescription2(pathToBuildProject);
        /* 
        string infoPlistFile = Path.Combine(pathToBuildProject, "Info.plist");
        AddCalendarUsageDescription(infoPlistFile);*/
        #endif
    }


    private static void AddWatchConnectivityFramework(string pathToBuildProject){

        PBXProject project = new PBXProject();
        string projectFile = PBXProject.GetPBXProjectPath(pathToBuildProject);
        project.ReadFromFile(projectFile);
        string targetGuid = project.TargetGuidByName(PBXProject.GetUnityTargetName());
        if(project.ContainsFramework(targetGuid, "WatchConnectivity.framework")) {
            Debug.Log("XCode already contains WatchConnectivity.framework");
        } else {
            project.AddFrameworkToProject(targetGuid, "WatchConnectivity.framework", false);
            Debug.Log("WatchConnectivity.framework added to XCode project");
        }
        project.WriteToFile(projectFile);
         
    }

    private static void AddCalendarUsageDescription2(string buildPath) {

        string infoPlistFile = Path.Combine(buildPath, "Info.plist");
        PlistDocument document = new PlistDocument();
        document.ReadFromFile(infoPlistFile);
        var rootElement = document.root;
        /* 
        foreach(var kvp in rootElement.values) {
            Debug.Log($"{kvp.Key}: {kvp.Value.ToString()}");
        }*/

        bool alreadyCasCalendars = false;
        foreach(var kvp in rootElement.values) {
            if(kvp.Key.ToLower() == "nscalendarsusagedescription") {
                alreadyCasCalendars = true;
                break;
            }
        }

        if(!alreadyCasCalendars) {
            rootElement.SetString("NSCalendarsUsageDescription", "BOS2 Calendar Usage");
            Debug.Log("Successfully write NSCalendarUsageDescription");
        } else {
            Debug.Log("Info.plist already contains NSCalendarUsageDescription");
        }

        if(!IsInfoPlistContainsKey(rootElement, "ITSAppUsesNonExemptEncryption".ToLower())) {
            rootElement.SetBoolean("ITSAppUsesNonExemptEncryption", false);
        } else {
            Debug.Log("Already contains ITSAppUsesNonExemptEncryption key");
        }

        if(false == IsInfoPlistContainsKey(rootElement, kNSLocationAlwaysUsageDescription.ToLower())) {
            rootElement.SetString(kNSLocationAlwaysUsageDescription, locationUsageDescription);
        } else {
            Debug.Log($"already contains key: {kNSLocationAlwaysUsageDescription}");
        }

        if(false == IsInfoPlistContainsKey(rootElement, kNSLocationWhenInUseUsageDescription.ToLower())) {
            rootElement.SetString(kNSLocationWhenInUseUsageDescription, locationUsageDescription);
        } else {
            Debug.Log($"already contains key: {kNSLocationWhenInUseUsageDescription}");
        }
        
        document.WriteToFile(infoPlistFile);

    }

    private static bool IsInfoPlistContainsKey(PlistElementDict dict, string key) {
        foreach(var kvp in dict.values) {
            if(kvp.Key.ToLower() == key) {
                return true;
            }
        }
        return false;
    }

        private static void AddCalendarUsageDescription(string file) {
            try {
                XDocument document = LoadDocument(file);
                if(HasCalendarElement(document)) {
                    Debug.Log("Already contains calendar usage description");
                    return;
                }
                AddCalendarElement(document);
                SaveDocument(file, document);
                Debug.Log("Calendar usage description successfully added");
            } catch(Exception exception) {
                Debug.Log("Exception when add calendar usage description to Info.plist");
                Debug.Log(exception.Message);
                Debug.Log(exception.StackTrace);
            }
        }

        private static bool HasCalendarElement(XDocument document) {
            var dictElement = document.Descendants("dict").FirstOrDefault();
            if(dictElement == null ) {
                throw new InvalidOperationException("dict element not founded in document");
            }
            foreach(XElement keyElement in dictElement.Elements("key")) {
                if(keyElement.Value.ToLower() == "nscalendarsusagedescription") {
                    return true;
                }
            }
            return false;
        }

        private static void AddCalendarElement(XDocument document) {
            var dictElement = document.Descendants("dict").FirstOrDefault();
            if(dictElement == null ) {
                throw new InvalidOperationException("dict element not founded in document");
            }
            XElement keyElement = new XElement("key", "NSCalendarsUsageDescription");
            XElement calendarStringElement = new XElement("string", "BOS2 Calendar Usage");
            dictElement.Add(keyElement);
            dictElement.Add(calendarStringElement);
        }

        private static XDocument LoadDocument(string file) {
            XDocument document = XDocument.Load(file);
            return document;  
        }

        private static void SaveDocument(string file, XDocument document) {
            document.Save(file);
        }
}
#endif