using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SwordWeapon : MonoBehaviour
{
	[Tooltip("How many frames to average over for computing velocity")]
	public int velocityAverageFrames = 5;
	[Tooltip("How many frames to average over for computing angular velocity")]
	public int angularVelocityAverageFrames = 11;

	public bool estimateOnAwake = true;

	private Coroutine routine;
	private int sampleCount;
	[SerializeField] private Vector3[] velocitySamples;
	private Vector3[] angularVelocitySamples;

	[SerializeField] GameObject AttackPoint = null; // あたり判定オブジェクト
	//[SerializeField] OVRInput.Controller Controller;
	[SerializeField] float fOnAttackVelocity = 3;



	//-------------------------------------------------
	void Awake()
	{
		velocitySamples = new Vector3[velocityAverageFrames];
		angularVelocitySamples = new Vector3[angularVelocityAverageFrames];


		if (estimateOnAwake)
		{
			BeginEstimatingVelocity();
		}
	}

	private void Update()
	{
		if (this.gameObject.activeSelf == true)
		{
			estimateOnAwake = this.gameObject.activeSelf;
			SlashAttack();
			//Weapon.enabled = true;
		}
		else
        {
			estimateOnAwake = this.gameObject.activeSelf;
		}

	}

	// 剣を振るスピードにより登録したコライダーを出現させる
	void SlashAttack()
	{
		if (GetVelocityEstimate().y > fOnAttackVelocity || GetVelocityEstimate().y < -fOnAttackVelocity)
		{
			Debug.Log("攻撃");
			AttackPoint.SetActive(true);

		}
		else
		{
			AttackPoint.SetActive(false);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{

		//if (collider.gameObject.tag == "Enemy")
		//{ 
		//	Damage(collider.gameObject.GetComponent<EnemyManager>());
		//	StartCoroutine(VibrateForSeconds(0.2f, 0.8f, 0.8f, Controller));
		//}

	}

	void Damage(IDamageble<float> damageble)
	{
		Debug.Log("ダメージ発生");
		damageble.AddDamage(10);
	}

	//-------------------------------------------------
	public void BeginEstimatingVelocity()
	{
		FinishEstimatingVelocity();

		routine = StartCoroutine(EstimateVelocityCoroutine());
	}

	//-------------------------------------------------
	public void FinishEstimatingVelocity()
	{
		if (routine != null)
		{
			StopCoroutine(routine);
			routine = null;
		}
	}


	// ジャイロセンサー
	public Vector3 GetVelocityEstimate()
	{
		// Compute average velocity
		Vector3 velocity = Vector3.zero;
		int velocitySampleCount = Mathf.Min(sampleCount, velocitySamples.Length);
		if (velocitySampleCount != 0)
		{
			for (int i = 0; i < velocitySampleCount; i++)
			{
				velocity += velocitySamples[i];

			}
			velocity *= (1.0f / velocitySampleCount);
		}

		return velocity;
	}


	//-------------------------------------------------
	public Vector3 GetAngularVelocityEstimate()
	{
		// Compute average angular velocity
		Vector3 angularVelocity = Vector3.zero;
		int angularVelocitySampleCount = Mathf.Min(sampleCount, angularVelocitySamples.Length);
		if (angularVelocitySampleCount != 0)
		{
			for (int i = 0; i < angularVelocitySampleCount; i++)
			{
				angularVelocity += angularVelocitySamples[i];
			}
			angularVelocity *= (1.0f / angularVelocitySampleCount);
		}

		return angularVelocity;
	}


	//-------------------------------------------------
	// 加速度チェック
	public Vector3 GetAccelerationEstimate()
	{
		Vector3 average = Vector3.zero;
		for (int i = 2 + sampleCount - velocitySamples.Length; i < sampleCount; i++)
		{
			if (i < 2)
				continue;

			int first = i - 2;
			int second = i - 1;

			Vector3 v1 = velocitySamples[first % velocitySamples.Length];
			Vector3 v2 = velocitySamples[second % velocitySamples.Length];
			average += v2 - v1;
		}
		average *= (1.0f / Time.deltaTime);
		return average;
	}

	//-------------------------------------------------
	private IEnumerator EstimateVelocityCoroutine()
	{
		sampleCount = 0;

		Vector3 previousPosition = transform.position;
		Quaternion previousRotation = transform.rotation;
		while (true)
		{
			yield return new WaitForEndOfFrame();

			float velocityFactor = 1.0f / Time.deltaTime;

			int v = sampleCount % velocitySamples.Length;
			int w = sampleCount % angularVelocitySamples.Length;
			sampleCount++;

			// Estimate linear velocity
			velocitySamples[v] = velocityFactor * (transform.position - previousPosition);

			// Estimate angular velocity
			Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(previousRotation);

			float theta = 2.0f * Mathf.Acos(Mathf.Clamp(deltaRotation.w, -1.0f, 1.0f));
			if (theta > Mathf.PI)
			{
				theta -= 2.0f * Mathf.PI;
			}

			Vector3 angularVelocity = new Vector3(deltaRotation.x, deltaRotation.y, deltaRotation.z);
			if (angularVelocity.sqrMagnitude > 0.0f)
			{
				angularVelocity = theta * velocityFactor * angularVelocity.normalized;
			}

			angularVelocitySamples[w] = angularVelocity;

			previousPosition = transform.position;
			previousRotation = transform.rotation;
		}
	}

	//-------------------------------------------------
	// 振動処理
	IEnumerator VibrateForSeconds(float duration, float frequency, float amplitude, OVRInput.Controller controller)
	{
		// 振動開始
		OVRInput.SetControllerVibration(frequency, amplitude, controller); //(振動数、振幅、右か左か)

		// 振動間隔分待つ
		yield return new WaitForSeconds(duration);

		// 振動終了
		OVRInput.SetControllerVibration(0, 0, controller);
	}
}

