using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteBridge.Lib.Contract;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.Service
{
    public partial class AccountEfficiencyCache
    {
        /*public static List<UserInfoEx> LoadUserInfoEx()
        {
            var result = new List<UserInfoEx>();
            using (var context = DatabaseContext.Instance.MakeTerminal())
            {
                try
                {
                    result.AddRange(context.USER_INFO.Select(info => new UserInfoEx(info, true)));
                }
                catch (Exception ex)
                {
                    Logger.Error("LoadUserInfoEx", ex);
                    return result;
                }
            }
            return result;
        }*/

        public UserInfoEx GetUserInfo(int id)
        {
            using (var context = DatabaseContext.Instance.Make())
            {
                try
                {
                    var info = context.USER_INFO.Find(id);
                    return info == null ? null : CreateUserInfoEx(info, true);
                }
                catch (Exception ex)
                {
                    Logger.Error("AccountEfficiencyCache.GetUserInfo", ex);
                    return null;
                }
            }
        }

        // если файловые данные (аватары, текст) не меняются, то передается null и действительная хеш-сумма
        public UserInfoEx SetUserInfo(UserInfoEx info)
        {
            UserInfoEx result;
            using (var context = DatabaseContext.Instance.Make())
            {
                try
                {
                    var dbUserInfo = context.USER_INFO.Find(info.Id);
                    var userInfoAdded = false;
                    var fileChanged = false;
                    if (dbUserInfo == null)
                    {
                        dbUserInfo = new USER_INFO();
                        dbUserInfo.Id = info.Id;
                        userInfoAdded = true;
                    }
                    // big avatar
                    if (dbUserInfo.AvatarBig == null)
                    {
                        if (info.AvatarBig != null) // insert
                            dbUserInfo.AvatarBig = context.FILE.Add(CreateBitmap(info.AvatarBig)).Name;
                    }
                    else if (dbUserInfo.FILE_BIG.HashCode != info.AvatarBigHashCode)
                        if (info.AvatarBig == null) // delete
                        {
                            context.FILE.Remove(dbUserInfo.FILE_BIG);
                            dbUserInfo.AvatarBig = null;
                        }
                        else // update
                        {
                            if (UpdateBitmap(dbUserInfo.FILE_BIG, info.AvatarBig))
                                fileChanged = true;
                        }
                    // small avatar
                    if (dbUserInfo.AvatarSmall == null)
                    {
                        if (info.AvatarSmall != null) // insert
                            dbUserInfo.AvatarSmall = context.FILE.Add(CreateBitmap(info.AvatarSmall)).Name;
                    }
                    else if (dbUserInfo.FILE_SMALL.HashCode != info.AvatarSmallHashCode)
                        if (info.AvatarSmall == null) // delete
                        {
                            context.FILE.Remove(dbUserInfo.FILE_SMALL);
                            dbUserInfo.AvatarSmall = null;
                        }
                        else // update
                        {
                            if (UpdateBitmap(dbUserInfo.FILE_SMALL, info.AvatarSmall))
                                fileChanged = true;
                        }
                    // about
                    if (dbUserInfo.About == null)
                    {
                        if (info.About != null) // insert
                            dbUserInfo.About = context.FILE.Add(CreateText(info.About)).Name;
                    }
                    else if (dbUserInfo.FILE_ABOUT.HashCode != info.AboutHashCode)
                        if (info.About == null) // delete
                        {
                            context.FILE.Remove(dbUserInfo.FILE_ABOUT);
                            dbUserInfo.About = null;
                        }
                        else // update
                        {
                            if (UpdateText(dbUserInfo.FILE_ABOUT, info.About))
                                fileChanged = true;
                        }
                    if(fileChanged)
                        context.SaveChanges();
                    dbUserInfo.Contacts = info.Contacts;
                    if (userInfoAdded)
                        context.USER_INFO.Add(dbUserInfo);
                    context.SaveChanges();
                    result = CreateUserInfoEx(dbUserInfo, false);
                }
                catch (Exception ex)
                {
                    Logger.Error("AccountEfficiencyCache.SetUserInfo", ex);
                    return null;
                }
            }
            return result;
        }

        public List<UserInfoEx> GetUsersBriefInfo(List<int> users)
        {
            var result = new List<UserInfoEx>();
            using (var context = DatabaseContext.Instance.Make())
            {
                try
                {
                    foreach (var user in users)
                    {
                        var info = context.USER_INFO.Find(user);
                        result.Add(info == null ? null : CreateUserInfoEx(info, false));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("AccountEfficiencyCache.GetUsersBriefInfo", ex);
                    return result;
                }
            }
            return result;
        }

        public List<string> GetFilesHashCodes(List<string> fileNames)
        {
            var result = new List<string>();
            using (var context = DatabaseContext.Instance.Make())
            {
                try
                {
                    foreach (var fileName in fileNames)
                    {
                        var file = context.FILE.Find(fileName);
                        result.Add(file == null ? null : file.HashCode);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("AccountEfficiencyCache.GetFilesHashCodes", ex);
                    return result;
                }
            }
            return result;
        }

        public List<byte[]> ReadFiles(List<string> fileNames)
        {
            var result = new List<byte[]>();
            using (var context = DatabaseContext.Instance.Make())
            {
                try
                {
                    foreach (var fileName in fileNames)
                    {
                        var file = context.FILE.Find(fileName);
                        if (file == null)
                        {
                            result.Add(null);
                            continue;
                        }
                        result.Add(file.Data.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("AccountEfficiencyCache.ReadFiles", ex);
                    return result;
                }
            }
            return result;
        }

        public bool WriteFile(string name, byte[] data)
        {
            using (var context = DatabaseContext.Instance.Make())
            {
                try
                {
                    var file = context.FILE.Find(name);
                    if (file == null)
                    {
                        context.FILE.Add(new FILE {Name = name, Data = data, HashCode = UserInfoEx.ComputeHash(data)});
                    }
                    else
                    {
                        file.Data = data;
                        file.HashCode = UserInfoEx.ComputeHash(data);
                    }
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error("AccountEfficiencyCache.WriteFile", ex);
                    return false;
                }
            }
            return true;
        }

        private UserInfoEx CreateUserInfoEx(USER_INFO info, bool full)
        {
            var result = new UserInfoEx();
            result.Id = info.Id;
            result.AvatarBigFileName = info.AvatarBig;
            // if full reading is enabled, then hash codes will be set automatically with setting data
            if (info.FILE_BIG != null)
            {
                if (full)
                    result.AvatarBigData = info.FILE_BIG.Data;
                else
                    result.AvatarBigHashCode = info.FILE_BIG.HashCode ?? UserInfoEx.ComputeHash(result.AvatarBigData);
            }
            result.AvatarSmallFileName = info.AvatarSmall;
            if (info.FILE_SMALL != null)
            {
                if (full)
                    result.AvatarSmallData = info.FILE_SMALL.Data;
                else
                    result.AvatarSmallHashCode = info.FILE_SMALL.HashCode ?? UserInfoEx.ComputeHash(result.AvatarSmallData);
            }
            result.AboutFileName = info.About;
            if (info.FILE_ABOUT != null)
            {
                if (full)
                    result.AboutData = info.FILE_ABOUT.Data;
                else
                    result.AboutHashCode = info.FILE_ABOUT.HashCode ?? UserInfoEx.ComputeHash(result.AboutData);
            }
            result.Contacts = info.Contacts;
            return result;
        }

        private static FILE CreateBitmap(Bitmap bitmap, string fileName = "")
        {
            var result = new FILE();
            if (string.IsNullOrEmpty(fileName))
                fileName = string.Format("{0}", Guid.NewGuid()) + ".png";
            result.Name = fileName;
            result.Data = UserInfoEx.GetBitmapData(bitmap);
            result.HashCode = UserInfoEx.ComputeHash(result.Data);
            return result;
        }

        private static bool UpdateBitmap(FILE file, Bitmap bitmap)
        {
            var data = UserInfoEx.GetBitmapData(bitmap);
            var hash = UserInfoEx.ComputeHash(data);
            if (file.HashCode == hash)
                return false;
            file.Data = data;
            file.HashCode = hash;
            return true;
        }

        private static FILE CreateText(string text, string fileName = "")
        {
            var result = new FILE();
            if (string.IsNullOrEmpty(fileName))
                fileName = string.Format("{0}", Guid.NewGuid()) + ".rtf";
            result.Name = fileName;
            result.Data = UserInfoEx.GetTextData(text);
            result.HashCode = UserInfoEx.ComputeHash(result.Data);
            return result;
        }

        private static bool UpdateText(FILE file, string text)
        {
            var data = UserInfoEx.GetTextData(text);
            var hash = UserInfoEx.ComputeHash(data);
            if (file.HashCode == hash)
                return false;
            file.Data = data;
            file.HashCode = hash;
            return true;
        }
    }
}
