using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class Player : NetworkBehaviour {


    public NetworkVariable<Vector3> PositionChange = new NetworkVariable<Vector3>();
    public NetworkVariable<Vector3> RotationChange = new NetworkVariable<Vector3>();
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(Color.red);
    public NetworkVariable<int> Score = new NetworkVariable<int>(0);
    public NetworkVariable<int> ChooseMat = new NetworkVariable<int>(0); 

    public TMPro.TMP_Text txtScoreDisplay;
    public TMPro.TMP_Text txtComDisplay;

    private GameManager _gameMgr;
    private Camera _camera;
    public float movementSpeed = .5f;
    private float rotationSpeed = 4f;
    private BulletSpawner _bulletSpawner;
    public int playerActive = 0;
    public int displayCom = 0;
    public Material chosenMaterial;

    private void Start() {
        //ApplyPlayerColor();
        //PlayerColor.OnValueChanged += OnPlayerColorChanged;
        ApplyTexture();
    }

    public override void OnNetworkSpawn() {
        _camera = transform.Find("Camera").GetComponent<Camera>();
        _camera.enabled = IsOwner;

        Score.OnValueChanged += ClientOnScoreChanged;
        _bulletSpawner = transform.Find("RArm").transform.Find("BulletSpawner").GetComponent<BulletSpawner>();
        
        if(IsHost)
        {
            _bulletSpawner.BulletDamage.Value = 1;
        }

        if(IsClient)
        {
            _bulletSpawner.BulletDamage.Value = 1;
        }

    }

    private void EnableDisplayComSTAT(Collider other)
    {
        other.GetComponent<NetworkObject>().Despawn();
        displayCom = 1;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(IsHost)
        {
            if(other.gameObject.CompareTag("DamageBoost"))
            {
                EnableDisplayComSTAT(other);
            }
        }
    }

    private void ClientOnScoreChanged(int previous, int current)
    {
        DisplayScore();
    }


    [ServerRpc]
    void RequestPositionForMovementServerRpc(Vector3 posChange, Vector3 rotChange) {
        if (!IsServer && !IsHost) return;

        PositionChange.Value = posChange;
        RotationChange.Value = rotChange;
    }

    private void HostHandleBulletCollision(GameObject bullet)
    {
        ulong owner = bullet.GetComponent<NetworkObject>().OwnerClientId;
        Player otherPlayer = NetworkManager.Singleton.ConnectedClients[owner].PlayerObject.GetComponent<Player>();
        otherPlayer.Score.Value += 1;

        Destroy(bullet);
    }

    [ServerRpc]
    public void RequestSetScoreServerRpc(int value)
    {
        Score.Value = value;
    }

    private void OnPlayerColorChanged(Color previous, Color current) {
        ApplyPlayerColor();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(IsHost)
        {
            if(collision.gameObject.CompareTag("Bullet"))
            {
                HostHandleBulletCollision(collision.gameObject);
            }
        }
    }



    public void ApplyPlayerColor() {
        GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
        transform.Find("LArm").GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
        transform.Find("RArm").GetComponent<MeshRenderer>().material.color = PlayerColor.Value;

    }

    public void ApplyTexture()
    {
        
        if(ChooseMat.Value == 1)
        {
            chosenMaterial = Resources.Load("Ground") as Material;
            transform.Find("RArm").transform.Find("ksvk").GetComponent<MeshRenderer>().material = chosenMaterial;
        }
        else if(ChooseMat.Value == 2)
        {
            chosenMaterial = Resources.Load("Mountain") as Material;
            transform.Find("RArm").transform.Find("ksvk").GetComponent<MeshRenderer>().material = chosenMaterial;
        }
        else
        {
            chosenMaterial = Resources.Load("Ground") as Material;
            transform.Find("RArm").transform.Find("ksvk").GetComponent<MeshRenderer>().material = chosenMaterial;
            chosenMaterial = Resources.Load("Leaf") as Material;
        }

        GetComponent<MeshRenderer>().material = chosenMaterial;
        transform.Find("LArm").GetComponent<MeshRenderer>().material = chosenMaterial;
        transform.Find("RArm").GetComponent<MeshRenderer>().material = chosenMaterial;
        transform.Find("REye").GetComponent<MeshRenderer>().material = chosenMaterial;
        transform.Find("LEye").GetComponent<MeshRenderer>().material = chosenMaterial;
        
    }

    // horiz changes y rotation or x movement if shift down, vertical moves forward and back.
    private Vector3[] CalcMovement() {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float x_move = 0.0f;
        float z_move = Input.GetAxis("Vertical");
        float y_rot = 0.0f;

        if (isShiftKeyDown) {
            x_move = Input.GetAxis("Horizontal");
        } else {
            y_rot = Input.GetAxis("Horizontal");
        }

        Vector3 moveVect = new Vector3(x_move, 0, z_move);
        moveVect *= movementSpeed;

        Vector3 rotVect = new Vector3(0, y_rot, 0);
        rotVect *= rotationSpeed;

        return new[] { moveVect, rotVect };
    }


    void Update() {
        if (IsOwner) {
            Vector3[] results = CalcMovement();
            RequestPositionForMovementServerRpc(results[0], results[1]);
            if (Input.GetButtonDown("Fire1"))
            {
                StartCoroutine(Shoot());
            }
        }

        if(!IsOwner || IsHost){
            transform.Translate(PositionChange.Value);
            transform.Rotate(RotationChange.Value);
        }

        if (displayCom == 0)
        {
            //Do nothing
        }
        else
        {
            ShowCOM();
        }

    }

    public void ShowCOM()
    {
        double distance = GetDistance();
        txtComDisplay.text = "Your opponent is " + distance.ToString()  + " units away";
    }

    private double GetDistance()
    {
        double calcDistance;

        //Get Player one coordinates
        double zOne = -200;
        double yOne = 0;
        double xOne = 200;

        //Get Player two coordinates
        double zTwo = 20;
        double yTwo = 0;
        double xTwo = 200;

        calcDistance = Mathf.Sqrt(Mathf.Pow((float)(zOne - zTwo), 2) + Mathf.Pow((float)(yOne - yTwo), 2) + Mathf.Pow((float)(xOne - xTwo), 2));

        return calcDistance;
    }

    public void DisplayScore()
    {
        if (Score.Value >= 10)
        {
            NetworkManager.SceneManager.LoadScene("GameOver", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            txtScoreDisplay.text = Score.Value.ToString();
        }
    }

    IEnumerator Shoot()
    {
        _bulletSpawner.FireServerRpc();
        yield return new WaitForSeconds(5);
    }
}