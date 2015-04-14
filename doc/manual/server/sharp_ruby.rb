require 'net/http'
require 'json'
require 'uri'

uri = URI.parse('http://10.5.237.10:8061/')
uri.query = URI.encode_www_form("formatquery" => 1, "balance" => 1)
puts uri

req = Net::HTTP::Post.new(uri.path + '?' + uri.query)
# ValueDate - в тиках+смещение временной зоны, переданное значение в примере
# 18.09.2013 17:14 с копейками
# см http://localhost:8061/?help=1

req.body = '[{"__type":"ChangeAccountBalanceQuery:#TradeSharp.Contract.WebContract","AccountId":3,"Amount":0.15,"ChangeType":1,"Description":null,"ValueDate":"\/Date(1379510002357+0400)\/"}]]'

res = Net::HTTP.start(uri.hostname, uri.port) do |http|
  http.request(req)
end

#objectResult = JSON.parse(res.body)
#puts JSON.pretty_generate(objectResult)

puts res.body