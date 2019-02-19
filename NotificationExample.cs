using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.SimpleAndroidNotifications
{
    public class NotificationExample : MonoBehaviour
    {
        public Toggle Toggle;

        public void Awake()
        {
            // TODO: Please note, that receiving callback will not work if your app was sleeping. It will only work if app was opened (not resumed) by clicking the notification.
            Toggle.isOn = NotificationManager.GetNotificationCallback() != null;
        }

        public void Rate()
        {
            Application.OpenURL("http://u3d.as/A6d");
        }

        public void OpenWiki()
        {
            Application.OpenURL("https://github.com/hippogamesunity/SimpleAndroidNotificationsPublic/wiki");
        }

        public void CancelAll()
        {
            NotificationManager.CancelAll();
        }

        public void ScheduleSimple(int seconds)
        {
            NotificationManager.Send(TimeSpan.FromSeconds(seconds), "Simple notification", "Please rate the asset on the Asset Store!", new Color(1f, 0.3f, 0.15f));
        }

        /// <summary>
        /// Note: as of API 19, all repeating alarms are inexact. If your application needs precise delivery times then it must use one-time exact alarms, rescheduling each time as described above.
        /// </summary>
        public void ScheduleNormal()
        {
            NotificationManager.SendWithAppIcon(TimeSpan.FromSeconds(5), "Notification", "Notification with app icon", new Color(0f, 0.6f, 1f), NotificationIcon.Message);
        }

        public void ScheduleRepeated()
        {
            var notificationParams = new NotificationParams
            {
                Id = NotificationIdHandler.GetNotificationId(),
                Delay = TimeSpan.FromSeconds(5),
                Title = "Repeated notification",
                Message = "Please rate the asset on the Asset Store!",
                Ticker = "This is repeated message ticker!",
                Sound = true,
                Vibrate = true,
                Vibration = new[] { 500, 500, 500, 500, 500, 500 },
                Light = true,
                LightOnMs = 1000,
                LightOffMs = 1000,
                LightColor = Color.magenta,
                SmallIcon = NotificationIcon.Skull,
                SmallIconColor = new Color(0f, 0.5f, 0f),
                LargeIcon = "app_icon",
                ExecuteMode = NotificationExecuteMode.Inexact,
                Repeat = true,
                RepeatInterval = TimeSpan.FromSeconds(30) // Don't use short intervals as repeated notifications are always inexact
            };

            NotificationManager.SendCustom(notificationParams);
        }

        public void ScheduleMultiline()
        {
            var notificationParams = new NotificationParams
            {
                Id = NotificationIdHandler.GetNotificationId(),
                Delay = TimeSpan.FromSeconds(5),
                Title = "Multiline notification",
                Message = "Line#1\nLine#2\nLine#3\nLine#4",
                Ticker = "This is multiline message ticker!",
                Multiline = true
            };

            NotificationManager.SendCustom(notificationParams);
        }

        public void ScheduleGrouped()
        {
            var id = NotificationIdHandler.GetNotificationId();
            var notificationParams = new NotificationParams
            {
                Id = id,
                GroupName = "Group",
                GroupSummary = "{0} new messages",
                Delay = TimeSpan.FromSeconds(5),
                Title = "Grouped notification",
                Message = "Message " + id,
                Ticker = "Please rate the asset on the Asset Store!"
            };

            NotificationManager.SendCustom(notificationParams);
        }

        public void ScheduleCustom()
        {
            var notificationParams = new NotificationParams
            {
                Id = NotificationIdHandler.GetNotificationId(),
                Delay = TimeSpan.FromSeconds(5),
                Title = "Notification with callback",
                Message = "Open app and check the checkbox!",
                Ticker = "Notification with callback",
                Sound = true,
                //CustomSound = "ding", // Please refer to plugin manual to learn how to add custom sounds
                Vibrate = true,
                Vibration = new[] { 500, 500, 500, 500, 500, 500 },
                Light = true,
                LightOnMs = 1000,
                LightOffMs = 1000,
                LightColor = Color.red,
                SmallIcon = NotificationIcon.Sync,
                SmallIconColor = new Color(0f, 0.5f, 0f),
                LargeIcon = "app_icon",
                ExecuteMode = NotificationExecuteMode.Inexact,
                CallbackData = "notification created at " + DateTime.Now
            };

            NotificationManager.SendCustom(notificationParams);
        }

        public void ScheduleWithChannel()
        {
            var notificationParams = new NotificationParams
            {
                Id = NotificationIdHandler.GetNotificationId(),
                Delay = TimeSpan.FromSeconds(5),
                Title = "Notification with news channel",
                Message = "Check the channel in your app settings!",
                Ticker = "Notification with news channel",
                ChannelId = "com.company.app.news",
                ChannelName = "News"
            };

            NotificationManager.SendCustom(notificationParams);
        }
    }
}