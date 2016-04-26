'use strict'

const _ = require('lodash')
const request = require('request')
const Promise = require('bluebird')

const req = {
  Case: "GetClientGarrison",
  Fields: [
    "bob"
  ]
}

let f = () => new Promise((resolve, reject) => {
  //request.post("http://localhost:8080/api", { json: req }, (err, resp, body) => {
  //  //console.log(err, body)
  //  err ? reject(err) : resolve()
  //})
  request.get('http://localhost:8080/chart', (err, resp, body) => {
    err ? reject(err) : resolve()
  })
})

let g = () => {
  let ti = null
  let msgs = 32

  return Promise.map(
    _.range(msgs),
    i => {
      let t = f()
      let ti = new Date()

      return t.then(() => {
        let tf = new Date()
        let dt = (tf - ti) / 1000.0
        return dt
      })
    }
  )
  .then(acc => {
    return {
      time: _.sum(acc) / 1000.0,
      msgs: msgs
    }
  })
}

let h = () => {
  let rounds = 16

  return Promise.map(
    _.range(rounds),
    i => g().then(res => {
      console.log('Round %s results', i)
      
      let mps = res.msgs / res.time

      console.log('mps = %s', mps)
      return res
    }),
    { concurrency: 1 }
  )
  .then(ls => {
    let results = _.reduce(ls, (res, l) => {
      return {
        time: res.time + l.time,
        msgs: res.msgs + l.msgs
      }
    }, { time: 0.0, msgs: 0.0 })
    let mps = results.msgs / results.time

    console.log('Final results :')
    console.log('mps = %s', mps)
  })
}

h()