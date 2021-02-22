using UnityEngine;
namespace PathTest
{
    public class FollowerTester : MonoBehaviour
    {
        public string Text
        {
            get
            {
                return transform.GetChild(0).GetComponent<TMPro.TextMeshPro>().text;
            }
            set
            {
                transform.GetChild(0).GetComponent<TMPro.TextMeshPro>().text = value;
            }
        }

        private PathFollower follower;
        private void Start()
        {
            follower = GetComponent<PathFollower>();
        }

        void LateUpdate()
        {
            if (follower.path.nodes.Count > 0)
            {
                Text = $"dist along: {follower.distAlong}\n" +
                    $"Baked points: {follower.path.bakedPoses.Count}\n" +
                    $"Current point: {(int)(follower.distAlong / follower.path.bakeResolution)}\n" +
                    $"Current point pos: {follower.path.bakedPoses[(int)(follower.distAlong / follower.path.bakeResolution)].position}";
            }
        }


        public void SetOffset(float offest)
        {
            follower.distAlong = offest * follower.path.bakedLength;
        }
    }
}
