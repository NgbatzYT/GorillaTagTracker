using BepInEx;
using BepInEx.Configuration;
using GorillaNetworking;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using UnityEngine;

namespace Tracker
{
    [BepInPlugin("ngbatz.tracker", "Tracker", "1.0.0")]
    public class Tracker : BaseUnityPlugin
    {

        private ConfigEntry<string> webhook;
        
        private string lastroom;

        void Start()
        {
            var harmony = Harmony.CreateAndPatchAll(GetType().Assembly, "ngbatz.tracker");
            GorillaTagger.OnPlayerSpawned(OnGameInitialized);
        }

        void OnGameInitialized()
        {
            webhook = Config.Bind("General", "WebHook", "https://discord.com/api/webhooks/yap", "Your discord webhook.");

            NetworkSystem.Instance.OnJoinedRoomEvent += OnRoomJoined;

            

            NetworkSystem.Instance.OnReturnedToSinglePlayer += OnRoomLeft;
        }
        void OnRoomJoined()
        {
            lastroom = NetworkSystem.Instance.RoomName;
            SendMessage($"Name: `{GorillaComputer.instance.savedName}`\nLobby: `{NetworkSystem.Instance.RoomName}`\nPlayers In Lobby: `{NetworkSystem.Instance.RoomPlayerCount}`", "Joined room");
        }

        void OnRoomLeft()
        {
            SendMessage($"`{GorillaComputer.instance.savedName}` Left the room `{lastroom}`", "Left room");
        }


        private string trackbutton = "Enable Tracking";
        private bool track = false;
        private bool show = true;
        private Rect window = new Rect(100, 100, 250, 100);

        void OnGUI()
        {
            if (!show) return;

            window = GUI.Window(0, window, Crreatewindow, "Tracker settings (close with p)");
        }

        void Crreatewindow(int id)
        {
            GUILayout.Space(10);

            if (GUILayout.Button(trackbutton, GUILayout.Height(30)))
            {
                track = !track;
                trackbutton = track ? "Enable Tracking" : "Disable Tracking";
            }

            GUI.DragWindow(new Rect(0, 0, window.width, 20));
        }

        public async void SendMessage(string message, string titl)
        {
            using HttpClient client = new();
            var payload = new
            {
                embeds = new[]
                {
                    new
                    {
                        title = titl,
                        description = message,
                        color = 10181046,
                    }
                }
            };




            string jsonPayload = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(webhook.Value, httpContent);
            }
            catch (Exception e)
            {
                Debug.Log($"[Error : Tracker] Error sending message: {e}");
            }
        }
    }
}