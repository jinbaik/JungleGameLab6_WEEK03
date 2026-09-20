using UnityEngine;

public class Resource_Base : MonoBehaviour
{
    private void Update()
    {
        this.transform.rotation = Quaternion.Euler(this.transform.rotation.eulerAngles + new Vector3(0, 1, 0));
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // 자원 데이터 관련 함수 호출
            Debug.Log("!");
            Destroy(this.gameObject);
        }
    }
}
