//============================================================
// シーン上のプレイヤー
//======================================================================
// 開発履歴
// 20220728:可用性向上のため再構築
//======================================================================
using UnityEngine;

namespace VR.Players
{
    public class MasterPlayer : MonoBehaviour
    {
        // 各クラス必要コンポーネント（Unity依存）
        CharacterController PlayerCharacter; 

        GameObject AnchorObject;                     // 浮遊移動用
        [SerializeField] GameObject CenterEyeAnchor; // カメラ（HMD本体）
        Vector3 InitirizeAnchorPos = new Vector3();

        // Playerのパラメータデータ
        [SerializeField] PlayerData Data; // スクリプタブル
        PlayerParameter Parameter;

        // 移動クラス
        StickMover NormalMove; // アナログスティックによる移動
        HoverMover HoverMove;  // 浮遊移動

        // 攻撃クラス

        // エフェクトクラス


        private void Start()
        {
            PlayerCharacter = GetComponent<CharacterController>();

            AnchorObject = new GameObject("AnchorObject");
            AnchorObject.transform.position = this.gameObject.transform.position;
            MoveAnchor moveAnchor = new MoveAnchor(CenterEyeAnchor, this.gameObject);
            moveAnchor = AnchorObject.AddComponent<MoveAnchor>();

            moveAnchor.Centereye = CenterEyeAnchor;
            moveAnchor.PlayerObj = this.gameObject;
            Debug.Log(moveAnchor.Centereye);


            Parameter = new PlayerParameter(Data);
            NormalMove = new StickMover();
            HoverMove = new HoverMover();

            
        }

        private void Update()
        {
            if (OVRInput.GetDown(OVRInput.RawButton.B))
            {
                // 現在位置を原点とし、移動開始の地点とする
                InitirizeAnchorPos = AnchorObject.transform.position;
            }

            // 浮遊移動
            HoverMove.HeadInclinationMove(this.gameObject ,PlayerCharacter, AnchorObject.transform.position, InitirizeAnchorPos,Parameter.fSpeed);
        }

        private void LateUpdate()
        {
            
        }
    }
}

