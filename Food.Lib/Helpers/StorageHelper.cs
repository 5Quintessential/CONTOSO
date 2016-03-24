﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Helpers
{
  public static class StorageHelper
  {
    #region Settings

    /// <summary>Returns if a setting is found in the specified storage strategy</summary>
    /// <param name="key">Path of the setting in storage</param>
    /// <param name="location">Location storage strategy</param>
    /// <returns>Boolean: true if found, false if not found</returns>
    public static bool SettingExists(string key, StorageStrategies location = StorageStrategies.Local)
    {
      switch (location)
      {
        case StorageStrategies.Local:
          return ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
        case StorageStrategies.Roaming:
          return ApplicationData.Current.RoamingSettings.Values.ContainsKey(key);
        default:
          throw new NotSupportedException(location.ToString());
      }
    }

    /// <summary>Reads and converts a setting into specified type T</summary>
    /// <typeparam name="T">Specified type into which to value is converted</typeparam>
    /// <param name="key">Path to the file in storage</param>
    /// <param name="otherwise">Return value if key is not found or convert fails</param>
    /// <param name="location">Location storage strategy</param>
    /// <returns>Specified type T</returns>
    public static T GetSetting<T>(string key, T otherwise = default(T), StorageStrategies location = StorageStrategies.Local)
    {
      try
      {
        if (!(SettingExists(key, location)))
          return otherwise;
        switch (location)
        {
          case StorageStrategies.Local:
            return (T)ApplicationData.Current.LocalSettings.Values[key.ToString()];
          case StorageStrategies.Roaming:
            return (T)ApplicationData.Current.RoamingSettings.Values[key.ToString()];
          default:
            throw new NotSupportedException(location.ToString());
        }
      }
      catch { /* error casting */ return otherwise; }
    }

    /// <summary>Serializes an object and write to file in specified storage strategy</summary>
    /// <typeparam name="T">Specified type of object to serialize</typeparam>
    /// <param name="key">Path to the file in storage</param>
    /// <param name="value">Instance of object to be serialized and written</param>
    /// <param name="location">Location storage strategy</param>
    public static void SetSetting<T>(string key, T value, StorageStrategies location = StorageStrategies.Local)
    {
      switch (location)
      {
        case StorageStrategies.Local:
          ApplicationData.Current.LocalSettings.Values[key.ToString()] = value;
          break;
        case StorageStrategies.Roaming:
          ApplicationData.Current.RoamingSettings.Values[key.ToString()] = value;
          break;
        default:
          throw new NotSupportedException(location.ToString());
      }
    }

    public static void DeleteSetting(string key, StorageStrategies location = StorageStrategies.Local)
    {
      switch (location)
      {
        case StorageStrategies.Local:
          ApplicationData.Current.LocalSettings.Values.Remove(key);
          break;
        case StorageStrategies.Roaming:
          ApplicationData.Current.RoamingSettings.Values.Remove(key);
          break;
        default:
          throw new NotSupportedException(location.ToString());
      }
    }

    #endregion

    #region File

    /// <summary>Returns if a file is found in the specified storage strategy</summary>
    /// <param name="key">Path of the file in storage</param>
    /// <param name="location">Location storage strategy</param>
    /// <returns>Boolean: true if found, false if not found</returns>
    public static async Task<bool> FileExistsAsync(string key, StorageStrategies location = StorageStrategies.Local)
    {
      return (await GetIfFileExistsAsync(key, location)) != null;
    }

    /// <summary>Deletes a file in the specified storage strategy</summary>
    /// <param name="key">Path of the file in storage</param>
    /// <param name="location">Location storage strategy</param>
    public static async Task<bool> DeleteFileAsync(string key, StorageStrategies location = StorageStrategies.Local)
    {
      var _File = await GetIfFileExistsAsync(key, location);
      if (_File != null)
        await _File.DeleteAsync();
      return !(await FileExistsAsync(key, location));
    }

    /// <summary>Reads and deserializes a file into specified type T</summary>
    /// <typeparam name="T">Specified type into which to deserialize file content</typeparam>
    /// <param name="key">Path to the file in storage</param>
    /// <param name="location">Location storage strategy</param>
    /// <returns>Specified type T</returns>
    public static async Task<T> ReadFileAsync<T>(string key, StorageStrategies location = StorageStrategies.Local)
    {
      try
      {
        // fetch file
        var _File = await GetIfFileExistsAsync(key, location);
        if (_File == null)
          return default(T);
        // read content
        var _String = await FileIO.ReadTextAsync(_File);
        // convert to obj
        var _Result = Deserialize<T>(_String);
        return _Result;
      }
      catch (Exception)
      {
        throw;
      }
    }

    /// <summary>Serializes an object and write to file in specified storage strategy</summary>
    /// <typeparam name="T">Specified type of object to serialize</typeparam>
    /// <param name="key">Path to the file in storage</param>
    /// <param name="value">Instance of object to be serialized and written</param>
    /// <param name="location">Location storage strategy</param>
    public static async Task<bool> WriteFileAsync<T>(string key, T value, StorageStrategies location = StorageStrategies.Local)
    {
      // create file
      var _File = await CreateFileAsync(key, location, CreationCollisionOption.ReplaceExisting);
      // convert to string
      var _String = Serialize(value);
      // save string to file
      await Windows.Storage.FileIO.WriteTextAsync(_File, _String);
      // result
      return await FileExistsAsync(key, location);
    }

    private static async Task<StorageFile> CreateFileAsync(string key, StorageStrategies location = StorageStrategies.Local,
       CreationCollisionOption option = CreationCollisionOption.OpenIfExists)
    {
      switch (location)
      {
        case StorageStrategies.Local:
          return await ApplicationData.Current.LocalFolder.CreateFileAsync(key, option);
        case StorageStrategies.Roaming:
          return await ApplicationData.Current.RoamingFolder.CreateFileAsync(key, option);
        case StorageStrategies.Temporary:
          return await ApplicationData.Current.TemporaryFolder.CreateFileAsync(key, option);
        default:
          throw new NotSupportedException(location.ToString());
      }
    }

    /// <summary>Returns a file if it is found in the specified storage strategy</summary>
    /// <param name="key">Path of the file in storage</param>
    /// <param name="location">Location storage strategy</param>
    /// <returns>StorageFile</returns>
    private static async Task<StorageFile> GetIfFileExistsAsync(string key, StorageStrategies location = StorageStrategies.Local,
        CreationCollisionOption option = CreationCollisionOption.FailIfExists)
    {
      StorageFile retval;
      try
      {
        switch (location)
        {
          case StorageStrategies.Local:
            retval = await ApplicationData.Current.LocalFolder.GetFileAsync(key);
            break;
          case StorageStrategies.Roaming: 
            retval = await ApplicationData.Current.RoamingFolder.GetFileAsync(key); 
            break;
          case StorageStrategies.Temporary:
            retval = await ApplicationData.Current.TemporaryFolder.GetFileAsync(key);
            break;
          default:
            throw new NotSupportedException(location.ToString());
        }
      }
      catch (FileNotFoundException)
      {
        Debug.WriteLine("GetIfFileExistsAsync:FileNotFoundException");
        return null;
      }

      return retval;
    }

    #endregion

    /// <summary>Serializes the specified object as a JSON string</summary>
    /// <param name="objectToSerialize">Specified object to serialize</param>
    /// <returns>JSON string of serialzied object</returns>
    private static string Serialize(object objectToSerialize)
    {
      using (MemoryStream _Stream = new MemoryStream())
      {
        try
        {
          var _Serializer = new DataContractJsonSerializer(objectToSerialize.GetType());
          _Serializer.WriteObject(_Stream, objectToSerialize);
          _Stream.Position = 0;
          StreamReader _Reader = new StreamReader(_Stream);
          return _Reader.ReadToEnd();
        }
        catch (Exception e)
        {
          Debug.WriteLine("Serialize:" + e.Message);
          return string.Empty;
        }
      }
    }

    /// <summary>Deserializes the JSON string as a specified object</summary>
    /// <typeparam name="T">Specified type of target object</typeparam>
    /// <param name="jsonString">JSON string source</param>
    /// <returns>Object of specied type</returns>
    private static T Deserialize<T>(string jsonString)
    {
      using (MemoryStream _Stream = new MemoryStream(Encoding.Unicode.GetBytes(jsonString)))
      {
        try
        {
          var _Serializer = new DataContractJsonSerializer(typeof(T));
          return (T)_Serializer.ReadObject(_Stream);
        }
        catch (Exception) { throw; }
      }
    }

    public enum StorageStrategies
    {
      /// <summary>Local, isolated folder</summary>
      Local,
      /// <summary>Cloud, isolated folder. 100k cumulative limit.</summary>
      Roaming,
      /// <summary>Local, temporary folder (not for settings)</summary>
      Temporary
    }

    public static async void DeleteFileFireAndForget(string key, StorageStrategies location)
    {
      await DeleteFileAsync(key, location);
    }

    public static async void WriteFileFireAndForget<T>(string key, T value, StorageStrategies location)
    {
      await WriteFileAsync(key, value, location);
    }
  }
}
