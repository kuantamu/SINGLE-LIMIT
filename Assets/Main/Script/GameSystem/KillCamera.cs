using UnityEngine;
using System.Collections;

public class KillCamera : MonoBehaviour
{
    [Header("�J�����ݒ�")]
    [SerializeField] Camera sabCamera;
    [SerializeField] ThirdPersonCamera TPC;
    [SerializeField] Transform player; // �v���C���[
    public float transitionDuration = 0.5f; // �J�������ړ����鎞��
    public float zoomDistance = 5.0f; // �G����̋���
    public float slowMotionTimeScale = 0.2f; // �X���[���[�V�����̑��x

    private bool isTriggered = false;

    [SerializeField] GameOverWin GameOverWin;
    public void ActivateKillCam(Transform targetEnemy)
    {
        if (isTriggered) return;
        isTriggered = true;
        TPC.enabled = false;
        StartCoroutine(SlowMotionRoutine());
        StartCoroutine(MoveCameraRoutine(targetEnemy));
    }

    private IEnumerator SlowMotionRoutine()
    {
        Time.timeScale = slowMotionTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        float startWaitTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startWaitTime < 5f)
        {
            yield return null;
        }
        TPC.enabled = true;
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;

        GameOverWin.StartWin();
    }

    private IEnumerator MoveCameraRoutine(Transform targetEnemy)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // �X���[�̉e����󂯂��Ɏ��Ԃ�i�߂�
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);

            Vector3 targetCamPos = targetEnemy.position + (targetEnemy.position - player.position).normalized * zoomDistance + Vector3.up * 2f;
            Quaternion targetRotation = Quaternion.LookRotation(targetEnemy.position - transform.position);

            transform.position = Vector3.Lerp(startPosition, targetCamPos, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }
    }
}
