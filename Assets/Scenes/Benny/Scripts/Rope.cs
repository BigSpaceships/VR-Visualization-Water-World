using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField] int attachSpeed;
    [SerializeField] float maxReleaseRadius;
    [SerializeField] Handedness handedness;
    public static SkiController leftController;
    public static SkiController rightController;
    enum Handedness { left, right };
    Transform myT;
    LineRenderer line;
    float attachProgress;
    bool attaching = true;
    Vector3 releaseTarget;
    public static Transform leftTarget;
    public static Transform rightTarget;
    public static bool paragliding;

    void Awake()
    {
        myT = transform;
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.widthMultiplier = 0.001f;
        releaseTarget = new Vector3(Random.Range(-maxReleaseRadius, maxReleaseRadius), 0, Random.Range(-maxReleaseRadius, maxReleaseRadius));
        Vector3 pos = myT.position;
        line.SetPositions(new Vector3[] { pos, pos + releaseTarget});
    }

    void Update()
    {
        //Released parachute
        if (!paragliding)
        {
            if (!attaching)
            {
                attaching = true;
                attachProgress = 0;
                releaseTarget = new Vector3(Random.Range(-maxReleaseRadius, maxReleaseRadius), 0, Random.Range(-maxReleaseRadius, maxReleaseRadius));
            }
            if (!Skier.EqualVectors(releaseTarget, line.GetPosition(1), 0.01f))
            {
                Vector3 pos = myT.position;
                line.SetPositions(new Vector3[] { pos, Vector3.Lerp(line.GetPosition(1), pos + releaseTarget, attachSpeed * Time.deltaTime) });
            }
            return;
        }

        //Selected parachute
        Vector3 startPos = myT.position;
        Vector3 targetPos;
        if (handedness == Handedness.left) targetPos = leftTarget.position;
        else targetPos = rightTarget.position;
        if (attaching)
        {
            attachProgress += attachSpeed * Time.deltaTime;
            attachProgress = Mathf.Clamp01(attachProgress);
            line.SetPositions(new Vector3[] { startPos, Vector3.Lerp(startPos, targetPos, attachProgress) });
            if (attachProgress >= 0.95f)
            {
                attaching = false;
                leftController.Animate("Select");
                rightController.Animate("Select");
            }
        }
        else line.SetPositions(new Vector3[] { startPos, targetPos });
     }
}
