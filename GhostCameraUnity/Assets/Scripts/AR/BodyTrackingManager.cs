using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class BodyTrackingController : MonoBehaviour
{
    [SerializeField]
    private ARHumanBodyManager bodyManager;

    // 귀신 모델 prefab을 Inspector에서 할당
    [SerializeField]
    private GameObject ghostPrefab;

    private GameObject spawnedGhost;

    void Awake()
    {
        // ARHumanBodyManager 컴포넌트를 자동으로 찾아 할당 (필요 시 Inspector에서 미리 연결 가능)
        if (bodyManager == null)
            bodyManager = GetComponent<ARHumanBodyManager>();
    }

    void OnEnable()
    {
        // 사람 인식 관련 이벤트에 구독
        bodyManager.trackablesChanged.AddListener(OnHumanBodiesChanged);
    }

    void OnDisable()
    {
        bodyManager.trackablesChanged.RemoveListener(OnHumanBodiesChanged);
    }

    void OnHumanBodiesChanged(ARTrackablesChangedEventArgs<ARHumanBody> args)
    {
        // 새로 인식된 또는 업데이트된 인체 데이터를 처리
        foreach (var humanBody in args.added)
        {
            ProcessBody(humanBody);
        }
        foreach (var humanBody in args.updated)
        {
            ProcessBody(humanBody);
        }
    }

    void ProcessBody(ARHumanBody humanBody)
    {
        // 인체 관절 데이터를 확인하여 어깨 위치를 추출
        if (humanBody.joints.IsCreated && humanBody.joints.Length > 0)
        {
            XRHumanBodyJoint leftShoulder;
            XRHumanBodyJoint rightShoulder;

            if (TryGetJoint(humanBody, HumanBodyJointType.LeftShoulder, out leftShoulder) &&
                TryGetJoint(humanBody, HumanBodyJointType.RightShoulder, out rightShoulder))
            {
                // 인체 로컬 좌표를 월드 좌표로 변환
                Vector3 leftPos = humanBody.transform.TransformPoint(leftShoulder.anchorPose.position);
                Vector3 rightPos = humanBody.transform.TransformPoint(rightShoulder.anchorPose.position);

                // 두 어깨의 중간 위치 계산 (귀신 모델 배치 기준)
                Vector3 midPoint = (leftPos + rightPos) * 0.5f;

                if (spawnedGhost == null)
                {
                    spawnedGhost = Instantiate(ghostPrefab, midPoint, Quaternion.identity);
                }
                else
                {
                    spawnedGhost.transform.position = midPoint;
                }
            }
        }
    }

    bool TryGetJoint(ARHumanBody humanBody, HumanBodyJointType jointType, out XRHumanBodyJoint joint)
    {
        joint = default;
        bool found = false;
        foreach (var j in humanBody.joints)
        {
            if (j.index == (int)jointType) // 🔥 최신 AR Foundation API에 맞게 수정
            {
                joint = j;
                found = true;
                break;
            }
        }
        return found;
    }
}

// 최신 API와 일치하도록 관절 ID를 정수로 비교하는 열거형(enum) 사용
public enum HumanBodyJointType
{
    LeftShoulder = 11,  // 실제 ARFoundation의 관절 ID에 맞게 수정해야 함
    RightShoulder = 12
}