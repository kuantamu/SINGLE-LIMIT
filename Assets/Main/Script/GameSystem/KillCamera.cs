using UnityEngine;
using System.Collections;

public class KillCamera : MonoBehaviour
{
    [Header("カメラ設定")]
    [SerializeField] Camera camera;
    [SerializeField] ThirdPersonCamera TPC;
    [SerializeField] Transform player; // プレイヤー
    public float transitionDuration = 0.5f; // カメラが移動する時間
    public float zoomDistance = 5.0f; // 敵からの距離
    public float slowMotionTimeScale = 0.2f; // スローモーションの速度

    private bool isTriggered = false;

    [SerializeField] GameOverWin GameOverWin;
    // 敵を撃破した時に呼ぶメソッド
    public void ActivateKillCam(Transform targetEnemy)
    {
        if (isTriggered) return;
        isTriggered = true;
        TPC.enabled = false;
        // スローモーション開始
        StartCoroutine(SlowMotionRoutine());
        // カメラの移動開始
        StartCoroutine(MoveCameraRoutine(targetEnemy));
    }

    private IEnumerator SlowMotionRoutine()
    {
        Time.timeScale = slowMotionTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // 一定時間スローを維持 (現実時間で待つため realtimeSinceStartup を使用)
        float startWaitTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startWaitTime < 5f)
        {
            Debug.Log(Time.realtimeSinceStartup - startWaitTime);
            yield return null;
        }
        TPC.enabled = true;
        // 時間を元に戻す
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;

        //ゲーム終了する
        GameOverWin.StartWin();
    }

    private IEnumerator MoveCameraRoutine(Transform targetEnemy)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // スローの影響を受けずに時間を進める
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);

            // 敵の少し後ろ、かつ敵とプレイヤーの間などにカメラを配置
            Vector3 targetCamPos = targetEnemy.position + (targetEnemy.position - player.position).normalized * zoomDistance + Vector3.up * 2f;
            Quaternion targetRotation = Quaternion.LookRotation(targetEnemy.position - transform.position);

            transform.position = Vector3.Lerp(startPosition, targetCamPos, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }
    }
}
