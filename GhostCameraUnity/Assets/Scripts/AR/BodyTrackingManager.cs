// Unity AR Foundation을 사용하여 인체 추적 및 귀신 모델 배치
// 이 스크립트는 ARHumanBodyManager를 사용하여 인체를 추적하고,
// 인체의 어깨 위치를 기준으로 귀신 모델을 배치합니다.
using System;
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
        // ARHumanBodyManager 컴포넌트를 자동으로 찾아 할당 (필요 시 Inspector에 미리 연결)
        if (bodyManager == null)
            bodyManager = GetComponent<ARHumanBodyManager>();
    }

    void OnEnable()
    {
        bodyManager.trackablesChanged.AddListener(OnHumanBodiesChanged);
    }

    void OnDisable()
    {
        bodyManager.trackablesChanged.RemoveListener(OnHumanBodiesChanged);
    }

    void OnHumanBodiesChanged(ARTrackablesChangedEventArgs<ARHumanBody> args)
    {
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
        // 여기서 humanBody의 관절 (예: 왼쪽/오른쪽 어깨)을 추출합니다.
        // API 버전에 따라 joints 컬렉션 접근 방법이 다를 수 있으므로, 실제 사용 중인 기능에 맞게 수정하세요.
        if (humanBody.joints.IsCreated && humanBody.joints.Length > 0) // ✅ 'Length'를 사용해야 함
        {
            XRHumanBodyJoint leftShoulder;
            XRHumanBodyJoint rightShoulder;

            if (TryGetJoint(humanBody, HumanBodyJointType.LeftShoulder, out leftShoulder) &&
                TryGetJoint(humanBody, HumanBodyJointType.RightShoulder, out rightShoulder))
            {
                // 인체 추적 데이터는 인체 로컬 좌표로 제공됩니다.
                // 이를 월드 좌표로 변환해줘야 정확한 위치 계산이 가능해요.
                Vector3 leftPos = humanBody.transform.TransformPoint(leftShoulder.anchorPose.position);
                Vector3 rightPos = humanBody.transform.TransformPoint(rightShoulder.anchorPose.position);

                // 두 어깨의 중간 위치 계산 (예: 귀신 모델 배치를 위한 기준점)
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

    // 관절 정보를 추출하는 예시 메서드 (사용 중인 ARFoundation API에 맞게 수정해야 합니다)
    bool TryGetJoint(ARHumanBody humanBody, HumanBodyJointType jointType, out XRHumanBodyJoint joint)
    {
        joint = default;
        bool found = false;
        foreach (var j in humanBody.joints)
        {
            if (j.index == (int)jointType) // ✅ 'index'를 사용해 관절 ID 비교
            {
                joint = j;
                found = true;
                break;
            }
        }
        return found;
    }
}

// 단순 예시용 enum (실제 ARFoundation에서 제공하는 관절 ID를 사용하세요)
public enum HumanBodyJointType
{
    LeftShoulder,
    RightShoulder,
    // 필요한 경우 다른 관절도 추가할 수 있습니다.
}