using UnityEngine;
using WebSocketSharp;
using System;

namespace bgc.unity.tool
{
    // 受信するギフトメッセージの型を定義
    [Serializable]
    public class GiftMessage
    {
        public string type;
        public int giftId;
        public int repeatCount;
        public string groupId;
        public string userId;
        public string secUid;
        public string uniqueId;
        public string nickname;
        public string profilePictureUrl;
        public int followRole;
        public UserBadge[] userBadges;
        public int[] userSceneTypes;
        public UserDetails userDetails;
        public FollowInfo followInfo;
        public bool isModerator;
        public bool isNewGifter;
        public bool isSubscriber;
        public int? topGifterRank;
        public int gifterLevel;
        public int teamMemberLevel;
        public string msgId;
        public string createTime;
        public string displayType;
        public string label;
        public bool repeatEnd;
        public Gift gift;
        public string describe;
        public int giftType;
        public int diamondCount;
        public string giftName;
        public string giftPictureUrl;
        public string timestamp;
        public string receiverUserId;
        public string profileName;
        public int combo;
    }

    [Serializable]
    public class UserBadge
    {
        public string type;
        public string privilegeId;
        public int level;
        public int badgeSceneType;
    }

    [Serializable]
    public class UserDetails
    {
        public string createTime;
        public string bioDescription;
        public string[] profilePictureUrls;
    }

    [Serializable]
    public class FollowInfo
    {
        public int followingCount;
        public int followerCount;
        public int followStatus;
        public int pushStatus;
    }

    [Serializable]
    public class Gift
    {
        public int gift_id;
        public int repeat_count;
        public int repeat_end;
        public int gift_type;
    }

    public class BgcTiktokWebSocket : MonoBehaviour
    {
        // 外部から設定可能な username（初期値 "default" なら接続しない）
        public static string Username { get; private set; } = "default";

        // 外部から username を設定するための関数
        public static void SetUsername(string username)
        {
            Username = username;
            Debug.Log("Username が設定されました: " + Username);
        }

        // 固定の WebSocket 接続先のベース URL
        // 接続先は "wss://tiktok-live-server-2.onrender.com/ws/" + username とする
        private string baseUrl = "wss://tiktok-live-server-2.onrender.com/ws/";

        // ギフトメッセージ受信時に発火するイベント
        public static event Action<GiftMessage> OnGiftReceived;

        private WebSocket ws;
        private bool isConnected = false;

        void Start()
        {
            Connect();
        }

        public void Connect()
        {
            // username が "default" の場合は接続しない
            if (Username == "default")
            {
                Debug.LogWarning("Usernameが'default'のため、接続しません。");
                return;
            }

            // 接続先URLを username を付加して生成
            string fullUrl = baseUrl + Username;

            if (isConnected)
            {
                Debug.LogWarning("既に接続済みです。");
                return;
            }

            Debug.Log("接続先URL: " + fullUrl);

            try
            {
                ws = new WebSocket(fullUrl);

                ws.OnOpen += (sender, e) => {
                    isConnected = true;
                    Debug.Log("WebSocketサーバーに接続しました: " + fullUrl);
                    SendApiKeyAndUsername();
                };

                ws.OnMessage += (sender, e) => {
                    // Debug.Log("WebSocketメッセージを受信: " + e.Data);
                    HandleWebSocketMessage(e.Data);
                };

                ws.OnError += (sender, e) => {
                    Debug.LogError("WebSocketエラー: " + e.Message);
                };

                ws.OnClose += (sender, e) => {
                    isConnected = false;
                    Debug.Log("WebSocketサーバーから切断されました。理由: " + e.Reason);
                };

                ws.Connect();
            }
            catch (Exception e)
            {
                Debug.LogError("WebSocketサーバーへの接続に失敗しました: " + e.Message);
            }
        }

        // API キーと Username を一緒に送信する関数
        private void SendApiKeyAndUsername()
        {
            string apiKey = "xxxxxxxxxxx";
            // 例として、JSON形式で送信（受信側に合わせてフォーマットを変更してください）
            string message = $"{{\"apiKey\": \"{apiKey}\", \"username\": \"{Username}\"}}";
            ws.Send(message);
            Debug.Log("API キーと Username を送信しました。");
        }

        // 受信したメッセージを解析して、ギフトメッセージの場合はイベントを発火する
        private void HandleWebSocketMessage(string message)
        {
            try
            {
                // JSON を GiftMessage 型に変換
                GiftMessage giftMsg = JsonUtility.FromJson<GiftMessage>(message);
                if (giftMsg != null && giftMsg.type == "gift")
                {
                    Debug.Log("ギフトメッセージを受信: " + giftMsg.giftName);
                    OnGiftReceived?.Invoke(giftMsg);
                }
                else
                {
                    Debug.Log("他のメッセージを受信: " + message);
                }
                // 他の messageType に応じた処理もここで追加可能
            }
            catch (Exception ex)
            {
                Debug.LogWarning("WebSocketメッセージの解析に失敗: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            if (ws != null && isConnected)
            {
                ws.Close();
            }
            else
            {
                Debug.LogWarning("接続がないか、既に切断されています。");
            }
        }

        void OnDestroy()
        {
            if (ws != null)
            {
                ws.Close();
                ws = null;
            }
        }
    }
}