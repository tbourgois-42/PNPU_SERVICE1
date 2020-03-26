import axios from 'axios'

class Webservice {

    static getInfoCLient(cb){
        axios
      .get('http://localhost:63267/Service1.svc/Alacon/1')
      .then((res) => {
        cb(res.data)
      })
      .catch((err) => {
        console.log(err)
      })
    }
}

module.exports = Webservice