namespace Bos {
    using Bos.Debug;
    using Ozh.Tools.Functional;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UDBG = UnityEngine.Debug;

#if UNITY_IOS
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;
using UnityEngine.iOS;
#endif

    public class LocalPushService : GameBehaviour, ILocalPushService {

        private bool isCleared = false;

        public void Setup(object data = null) {
#if UNITY_IOS
        NotificationServices.RegisterForNotifications(NotificationType.Alert | NotificationType.Badge | NotificationType.Sound, registerForRemote: true);
#endif
        }

        private void OnApplicationPause(bool pause) {
            UpdateResume(pause);
        }

        private void OnApplicationFocus(bool focus) {
            UpdateResume(!focus);
        }

        public void UpdateResume(bool pause) {
            //UDBG.Log($"{nameof(LocalPushService)}.{nameof(UpdateResume)}() => {pause}");
            Schedule(pause);
        }

        private void OnApplicationQuit() {
            ScheduleLocalNotifications();
        }

        private void Schedule(bool isSchedule ) {
            if (isSchedule) {
                ScheduleLocalNotifications();
            } else {
                ClearNotifications();
            }
        }



        public void ScheduleLocalNotifications() {
            if (isCleared) {
                
                
                // secretary notify
                if (ResourceService != null && ResourceService.IsLoaded && Planets != null && Planets.IsLoaded) {
                    //reports push
                    if (Planets?.IsMoonOpened ?? false) {
                        ScheduleNotification(new Notification {
                            Title = LocalizationObj.GetString("GAME.TITLE"),
                            Content = LocalizationObj.GetString("nt_secr"),
                            Delay = TimeSpan.FromHours(6)
                        });
                    }

                    //broken transport push
                    /*if (Planets?.IsMarsOpened ?? false) {
                        ScheduleNotification(new Notification {
                            Title = LocalizationObj.GetString("GAME.TITLE"),
                            Content = LocalizationObj.GetString("nt_mech"),
                            Delay = TimeSpan.FromHours(10)
                        });
                    }*/
                }

                
                // bank notify
                Services.BankService.TimeToFullBank.Match(() => {
                    //UDBG.Log($"Bank not opened...Push not sended");
                    return F.None;
                }, interval =>
                {
                    ScheduleNotification(new Notification
                    {
                        Title = LocalizationObj.GetString("GAME.TITLE"),
                        Content = LocalizationObj.GetString("nt_bank_is_full"),
                        Delay = TimeSpan.FromSeconds(CalculateNonInvasiveTime((int) TimeSpan.FromHours(14).TotalSeconds))

                    });
                    //UDBG.Log($"Schedule bank notification after {interval.ToString()}");
                    return F.Some(interval);
                });
                   
                // daily bonus streak notify
                 ScheduleNotification(new Notification() {
                        Delay = TimeSpan.FromSeconds(CalculateNonInvasiveTime((int)TimeSpan.FromHours(23).TotalSeconds)),
                        Content = Services.ResourceService.Localization.GetString("lbl_note_1"),
                        Title = Services.ResourceService.Localization.GetString("GAME.TITLE")
                  });   
                

                if (Services.InvestorService.GetSecuritiesCountFromInvestors() > 1000 &&
                    Services.InvestorService.TriesCount > 0) {
                    ScheduleNotification(new Notification() {
                        Delay = TimeSpan.FromSeconds(CalculateNonInvasiveTime((int) TimeSpan.FromHours(48).TotalSeconds)),
                        Content = Services.ResourceService.Localization.GetString("lbl_note_4"),
                        Title = Services.ResourceService.Localization.GetString("GAME.TITLE")
                    });
                }

                for (int i = 1; i < 11; i++)
                {
                    ScheduleNotification(new Notification {
                        Title = LocalizationObj.GetString("GAME.TITLE"),
                        Content = LocalizationObj.GetString("nt_12hrs"),
                        Delay = TimeSpan.FromSeconds(CalculateNonInvasiveTime((int)TimeSpan.FromHours(96 * i).TotalSeconds)),
                    }); 
                }
                isCleared = false;
            }
        }


        public void ClearNotifications() {
            if (!isCleared) {
#if UNITY_IOS
        NotificationServices.CancelAllLocalNotifications();
#endif

#if UNITY_ANDROID
                Assets.SimpleAndroidNotifications.NotificationManager.CancelAll();
#endif
                isCleared = true;
            }
        }

        private void ScheduleNotification(Notification notification) {
            //UDBG.Log(notification.ToString());

#if UNITY_ANDROID

            Assets.SimpleAndroidNotifications.NotificationManager.SendWithAppIcon(notification.Delay, notification.Title, notification.Content, Color.black, Assets.SimpleAndroidNotifications.NotificationIcon.Dollar);

#endif

#if UNITY_IOS
        NotificationServices.ScheduleLocalNotification(new UnityEngine.iOS.LocalNotification()
        {
            fireDate = DateTime.Now.Add(notification.Delay),
            alertBody = notification.Content,
            soundName = "default"
        });

#endif
        }

        private bool IsInQuietInterval(DateTime dt) {
            return dt.Hour >= 22 || dt.Hour <= 8;
        }


        private int CalculateNonInvasiveTime(int idealTime) {
            var dt = DateTime.Now.AddSeconds(idealTime);
            if (IsInQuietInterval(dt)) {
                for (int i = 0; i < 24; i++) {
                    var newTime = dt.AddHours(i);
                    if (!IsInQuietInterval(newTime)) {
                        return (int)(newTime - DateTime.Now).TotalSeconds;
                    }
                }
            }
            return idealTime;
        }
    }

    public interface ILocalPushService : IGameService {

    }

    public class Notification {
        public TimeSpan Delay { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }

        public override string ToString() {
            var localizationObj = GameServices.Instance.ResourceService.Localization;

            return $"Scheduled notification at {Delay.ToString().Colored(ConsoleTextColor.yellow).Bold()}, \n" +
                $"Title => {Title} Content => {Content}";
        }
    }
}