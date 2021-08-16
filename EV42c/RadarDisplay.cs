using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EV42c
{
    public class RadarDisplay : MonoBehaviour
    {
        public float maxRadarRange = 29632;
        public float maxLocalScale = 0.7f;
        public float uiHeight = 0.7f;
        public Transform radarLine;
        public float rotationSpeed;

        public Text rangeText;

        public List<Actor> detectedActors = new List<Actor>();

        private List<GameObject> detectedIcons = new List<GameObject>();

        private Dictionary<int, RadarDisplay.UIRadarContact> contacts = new Dictionary<int, RadarDisplay.UIRadarContact>();
        private RadarDisplay.UIRadarContact[] cArray = new RadarDisplay.UIRadarContact[100];

        public Actor myActor;

        public GameObject enemyIcon;
        public GameObject friendlyIcon;
        public GameObject missileIcon;

        public Transform canvasTransform;

        public Radar radar;


        private Text braText;
        private Actor braActor;

        private MeasurementManager measurements;

        private GameObject lineIcon;

        void Awake()
        {
            this.radarLine = Main.GetChildWithName(base.gameObject, "SweepLine").transform;

            this.enemyIcon = Main.GetChildWithName(base.gameObject, "EnemyObj");
            this.friendlyIcon = Main.GetChildWithName(base.gameObject, "FriendlyObj");
            this.missileIcon = Main.GetChildWithName(base.gameObject, "MissileObj");

            this.lineIcon = GameObject.Instantiate(this.missileIcon);
            this.lineIcon.transform.SetParent(this.canvasTransform);
            this.lineIcon.transform.localScale = this.missileIcon.transform.localScale * 2;
            this.lineIcon.transform.localEulerAngles = Vector3.zero;
            this.lineIcon.SetActive(true);

            this.canvasTransform = base.gameObject.GetComponentInChildren<Canvas>(true).transform;

            this.braText = Main.GetChildWithName(base.gameObject, "BRA_Text").GetComponent<Text>();

            this.measurements = this.myActor.gameObject.GetComponentInChildren<MeasurementManager>(true);

        }
        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("Attempting to add ondetected actor");
            this.radar.OnDetectedActor += this.OnDetectActor;
            Debug.Log("Added ondetected actor!");
        }

        // Update is called once per frame
        void Update()
        {
            this.radarLine.localEulerAngles = new Vector3(0, 0, -90 - radar.currentAngle);

            UpdateLocks();
           
           // Vector3 actPos = new Vector3(this.detectedActors[0].transform.position.x, this.myActor.transform.position.y, this.detectedActors[0].transform.position.z);

           // Debug.DrawLine(this.myActor.transform.position, actPos, Color.green);
            //this.updateIcons();
        }

        private void EnableLineIcon()
        {
            this.lineIcon.SetActive(true);
        }

        private void DisableLineIcon( )
        {
            this.lineIcon.SetActive(false);
        }

        private void UpdateLineIcon(VRInteractable interactable)
        {
            FlightLogger.Log("1");
            Transform fingerTf = interactable.activeController.gloveAnimation.fingerTipTf;
            FlightLogger.Log("2");
            Vector3 glovePos = interactable.activeController.gloveAnimation.fingerTipTf.position;
            FlightLogger.Log("3");

            //Vector3 localPos = this.lineIcon.transform.parent.InverseTransformPoint(interactable.activeController.gloveAnimation.fingerTipTf.position);
            FlightLogger.Log("4");
            this.lineIcon.transform.position = fingerTf.position;
            FlightLogger.Log("5");
        }
        private void SelectBRA(Actor actor)
        {
            this.braActor = actor;
        }

        private void DisplayBRA(Actor actor)
        {
            if(this.braActor != null && actor == this.braActor)
            {

                float b = Mathf.RoundToInt(VectorUtils.Bearing(this.myActor.position, this.braActor.position));
                float r = Mathf.RoundToInt(this.measurements.ConvertedDistance(Vector3.Distance(this.myActor.position, this.braActor.position)));
                float a = Mathf.RoundToInt(this.measurements.ConvertedAltitude(WaterPhysics.GetAltitude(this.braActor.position)));
                this.braText.text = $"B:{b}\nR:{r}\nA:{a}";
            }
        }

        private void OnDetectActor(Actor a)
        {
        
            if (!a)
            {
                return;
            }

            int actorID = a.actorID;
            if (this.contacts.ContainsKey(actorID))
            {
                //FlightLogger.Log("Found actor: " + a.actorName);
                this.UpdateIcon(this.contacts[actorID], true);
                DisplayBRA(a);
                return;
            }

            RadarDisplay.UIRadarContact uiradarContact = new RadarDisplay.UIRadarContact();
            try
            {
                uiradarContact.detectedPosition = new FixedPoint(a.position);
            }
            catch (MissingReferenceException)
            {
                uiradarContact.detectedPosition = new FixedPoint(a.transform.position);
            }
            uiradarContact.actor = a;
            if (a.finalCombatRole == Actor.Roles.Missile)
            {
                uiradarContact.iconObject = GameObject.Instantiate(this.missileIcon);
                FlightLogger.Log("Ayo creating missile icon");
            }
            else if (a.team == Teams.Enemy)
            {
                uiradarContact.iconObject = GameObject.Instantiate(this.enemyIcon);
                FlightLogger.Log("Ayo creating enemy icon");
            }
            else
            {
                uiradarContact.iconObject = GameObject.Instantiate(this.friendlyIcon);
                FlightLogger.Log("Ayo creating friendly icon");
            }

            //Adds a vrinteractble to select the targets
            VRInteractable interact = uiradarContact.iconObject.gameObject.AddComponent<VRInteractable>();
            interact.interactableName = a.actorName;
            interact.button = VRInteractable.Buttons.Trigger;
            interact.radius = 0;
            interact.useRect = true;
            interact.rect.center = Vector3.zero;
            interact.rect.extents = new Vector3(50, 50, 60);
            interact.requireMotion = false;
            interact.toggle = false;
            interact.tapOrHold = false;
            interact.sqrRadius = 0;
            interact.OnInteract = new UnityEvent();
            interact.OnStopInteract = new UnityEvent();
            interact.OnInteracting = new UnityEvent();

            interact.OnInteract.AddListener(delegate { SelectBRA(a); });
            interact.OnInteract.AddListener(EnableLineIcon);

            interact.OnInteract.AddListener(DisableLineIcon);

            interact.OnInteracting.AddListener(delegate { UpdateLineIcon(interact); });

            uiradarContact.iconObject.SetActive(true);
            uiradarContact.iconObject.transform.SetParent(this.canvasTransform);
            uiradarContact.iconObject.transform.localScale = this.missileIcon.transform.localScale;
            uiradarContact.iconObject.transform.localEulerAngles = Vector3.zero;
            uiradarContact.actorID = actorID;
            uiradarContact.active = true;

            this.UpdateIcon(uiradarContact, true);
           

            this.contacts.Add(actorID, uiradarContact);
            FlightLogger.Log("added contact " + uiradarContact.actor.actorName);

        }

        private void UpdateIcon(RadarDisplay.UIRadarContact contact, bool resetTime )
        {
            if (resetTime)
            {
                contact.detectedPosition.point = contact.actor.position;
                contact.detectedVelocity = contact.actor.velocity;
                contact.timeFound = Time.time;
            }

            //change actor icon here to actual actor position (may have to make new class
            Vector3 worldToRadar = WorldToRadarPoint(contact.detectedPosition.point);

            float distance = worldToRadar.magnitude;
            float clampedDistance = Mathf.Clamp(distance, 0, maxLocalScale);

            contact.iconObject.transform.localPosition = worldToRadar.normalized * clampedDistance;
        }

        private void UpdateLocks()
        {
            if (!this.radar.radarEnabled)
            {
                return;
            }
            int count = this.contacts.Count;
            if (this.cArray.Length < count)
            {
                this.cArray = new RadarDisplay.UIRadarContact[count * 2];
            }
            this.contacts.Values.CopyTo(this.cArray, 0);
            for (int i = 0; i < count; i++)
            {
                RadarDisplay.UIRadarContact uiradarContact = this.cArray[i];
                if (uiradarContact != null && (uiradarContact.actor == null || Time.time - uiradarContact.timeFound > this.radar.detectionPersistanceTime || !uiradarContact.actor.alive))
                {
                    FlightLogger.Log("Removing contact!");
                    this.contacts.Remove(uiradarContact.actorID);
                    uiradarContact.iconObject.SetActive(false);
                    FlightLogger.Log($"Setting {uiradarContact.actor.actorName} icon to false!");
                    uiradarContact.active = false;
                }
            }

        }

        private Vector3 WorldToRadarPoint(Vector3 worldPoint)
        {
            worldPoint.y = this.myActor.transform.position.y;
            Vector3 forward = this.myActor.transform.forward;
            forward.y = 0f;
            float angle = VectorUtils.SignedAngle(forward, worldPoint - this.myActor.transform.position, Vector3.Cross(Vector3.up, forward));
            float num = Vector3.Distance(worldPoint, this.myActor.transform.position);
            num *= this.uiHeight / this.maxRadarRange;
            return Quaternion.AngleAxis(angle, -Vector3.forward) * new Vector3(0f, num, 0f);
        }

      
        public class UIRadarContact
        {
           
    
            public Actor actor;

            public int actorID;

            public float timeFound;
   
            public FixedPoint detectedPosition;

            public Vector3 detectedVelocity;

            public GameObject iconObject;

            public bool active = true;
        }

    }
}
