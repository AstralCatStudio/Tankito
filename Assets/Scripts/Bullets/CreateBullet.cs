using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using Unity.Netcode;
using UnityEngine;

namespace Tankito
{
    public class CreateBullet : NetworkBehaviour
    {
        public GameObject bulletPrefab;
        [SerializeField]
        public BulletProperties m_bulletProperties;
        [SerializeField]
        float interval = 1;
        float timer = 0;
        private

        void Start()
        {
            
        }
        public void Shoot()
        {
            // Probablemente sea razonable añadir un intervalo de buffer de inputs, de modo que si disparas justo antes de que se acabe el cooldown, dispare en cuanto se pueda. - Bernat

            if (NetworkManager.Singleton.IsServer)
            {
                /*

                El problema es que en los clientes no se estan spawneando las balas con las propiedades que se le asocian al objeto en la linea de SetProperties.
                Esto se debe a que despues en Spawn() realmente no se esta espawneando si no que lo que hace es conseguir una referencia del prefab pooleado, y recibe
                eso como instancia, por lo que no replica el network object 1:1 como ocurriria al spawnearse de verdad.
                
                Hay 2 opciones para solventarlo:

                1 - En el prefab instance handler o como se llame, osea lo que devuelve en el cliente una instancia del prefab que se "spawnea" incluir un override de copiar de los atributos,
                    replicando el comportamiento de spawnear habitual (propaga todos los campos de los componentes del network object, osea lo spawnea 1:1).

                2 - Mantener las propiedades de los tanques/disparadores (m_bulletProperties) que spawnean los proyectiles sincronizados, y cuando se spawnean en cada cliente, copiar
                    estas propiedades del disparador asociado.

                La opcion 1 ofrece una interfaz mas lisa, ya que replicaria para los objetos pooleados exactamente el mismo comportamiento que spawnear objetos no pooleados en la red.
                Pero la cosa esta en que esa implementacion intensificaria mucho el uso de la red, ya que cada vez que se spawnea un proyectil (o lo que sea) tendria que mandar el servidor
                a cada cliente los parametros del proyectil. Lo cual se repitiria constantemente con los mismos parametros, ya que no son algo que se vea alterado cada vez que se dispara, si no que,
                al menos en un principio, se establece entre rondas ya que viene definido por los power ups del jugador (o en un futuro posiblemente algunos factores de partida,
                como el tiempo de ronda restante, por ejemplo).

                La opcion 2 puede ser un poco mas fragil y liosa ya que tendremos que asegurar que los parametros de cada disparador/tanque se mantengan sincronizados a lo largo de la partida,
                pero creo que es la opcion mas favorable ya que esta sincronizacion estaria concentrada en los momentos entre ronda, en los que de por si apenas hay intercambio de datos en la red,
                al contrario que ocurre durante la ronda como tal. Luego tambien esta la cosa de que se mete la logica de tener que obtener los parametros del disparador dentro de la bala,
                pero es tan simple como meterlo en la funcion base de Init() de la clase ABullet, sin necesidad de reescribirlo en ningun otro derivado de la clase.

                */
                if (timer > interval)
                {
                    timer = 0;
                    m_bulletProperties.direction = transform.right;
                    m_bulletProperties.startingPosition = transform.position;
                    var newBullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab, transform.position, transform.rotation).gameObject;
                    newBullet.GetComponent<ABullet>().SetProperties(m_bulletProperties);
                    //newBullet.GetComponent<ABullet>().m_ownerID = gameObject.GetComponent<NetworkObject>().OwnerClientId;
                    newBullet.GetComponent<NetworkObject>().Spawn();
                    newBullet.GetComponent<BaseBullet>()?.Init();
                }
            }
            else
            {
                Debug.Log("TODO: Implement client fire RPC Call (with server-side verification of the action, cooldown checks, etc...)");
                // TIP: https://discussions.unity.com/t/request-the-server-to-spawn-an-object-then-return-a-reference-to-that-object-to-the-client/907767
            }
        }
        void Update()
        {
            timer += Time.deltaTime;
        }
    }
}
