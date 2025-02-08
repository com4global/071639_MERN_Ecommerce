(() => {
    var w = Object.create;
    var f = Object.defineProperty,
        A = Object.defineProperties,
        E = Object.getOwnPropertyDescriptor,
        M = Object.getOwnPropertyDescriptors,
        P = Object.getOwnPropertyNames,
        S = Object.getOwnPropertySymbols,
        B = Object.getPrototypeOf,
        h = Object.prototype.hasOwnProperty,
        C = Object.prototype.propertyIsEnumerable;
    var D = (t, e, o) => e in t ? f(t, e, {
            enumerable: !0,
            configurable: !0,
            writable: !0,
            value: o
        }) : t[e] = o,
        g = (t, e) => {
            for (var o in e || (e = {})) h.call(e, o) && D(t, o, e[o]);
            if (S)
                for (var o of S(e)) C.call(e, o) && D(t, o, e[o]);
            return t
        },
        O = (t, e) => A(t, M(e));
    var _ = (t, e) => () => (t && (e = t(t = 0)), e);
    var X = (t, e) => () => (e || t((e = {
        exports: {}
    }).exports, e), e.exports);
    var $ = (t, e, o, p) => {
        if (e && typeof e == "object" || typeof e == "function")
            for (let a of P(e)) !h.call(t, a) && a !== o && f(t, a, {
                get: () => e[a],
                enumerable: !(p = E(e, a)) || p.enumerable
            });
        return t
    };
    var j = (t, e, o) => (o = t != null ? w(B(t)) : {}, $(e || !t || !t.__esModule ? f(o, "default", {
        value: t,
        enumerable: !0
    }) : o, t));
    var N = (t, e, o) => new Promise((p, a) => {
        var c = n => {
                try {
                    u(o.next(n))
                } catch (s) {
                    a(s)
                }
            },
            d = n => {
                try {
                    u(o.throw(n))
                } catch (s) {
                    a(s)
                }
            },
            u = n => n.done ? p(n.value) : Promise.resolve(n.value).then(c, d);
        u((o = o.apply(t, e)).next())
    });
    var x, I = _(() => {
        x = "WebPixel::Render"
    });
    var b, J = _(() => {
        I();
        b = t => shopify.extend(x, t)
    });
    var T = _(() => {
        J()
    });
    var k = _(() => {
        T()
    });
    var v = X(y => {
        k();
        b(p => N(y, [p], function*({
            settings: t,
            analytics: e,
            browser: o
        }) {
            var a = yield o.localStorage.getItem("er_client_id");
            a || (a = Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15), o.localStorage.setItem("er_client_id", a));
            var c, d = !1;
            let u = `https://pixel.ecomrise.app/v1/analytic?id=${t.accountID}&shopify_domain=${t.shopify_domain}`,
                n = i => /(tablet|ipad|playbook|silk)|(android(?!.*mobi))/i.test(i + "") ? "tablet" : /Mobile|iP(hone|od)|Android|BlackBerry|IEMobile|Kindle|Silk-Accelerated|(hpw|web)OS|Opera M(obi|ini)/.test(i + "") ? "mobile" : "desktop";

            function s(i) {
                let r = JSON.parse(JSON.stringify(i)),
                    l = r.context;
                r.context = {
                    userAgent: l.navigator.userAgent,
                    url: l.window.location.href,
                    referrer: l.document.referrer,
                    title: l.document.title,
                    device: n(l.userAgent),
                    er_client_id: a
                }, r.client_id = r.clientId, r.Data = r.customData, delete r.clientId, delete r.customData, r.Data === void 0 && (r.Data = c);
                let m = O(g(g({}, r), c), {
                    store_id: parseInt(t.accountID)
                });
                r.Data && (r.Data.extension && (m.extension = r.Data.extension), r.Data.productId && (m.productId = r.Data.productId), r.Data.rule_id && (m.rule_id = r.Data.rule_id)), console.log("sendData", m), fetch(u, {
                    method: "POST",
                    cache: "no-cache",
                    mode: "no-cors",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    keepalive: !0,
                    body: JSON.stringify(m)
                })
            }
            e.subscribe("er_product_viewed", i => {
                c = JSON.parse(JSON.stringify(i.customData)), i.name = "product_viewed", setTimeout(() => s(i), 500)
            }), e.subscribe("er_page_viewed", i => {
                c = JSON.parse(JSON.stringify(i.customData)), i.name = "page_viewed", setTimeout(() => s(i), 500)
            }), e.subscribe("er_add_to_cart", i => {
                c = JSON.parse(JSON.stringify(i.customData)), d = !0
            }), e.subscribe("product_added_to_cart", i => {
                console.log("clickAtc", d), d && s(i)
            }), e.subscribe("er_checkout_start", i => {
                c = JSON.parse(JSON.stringify(i.customData)), i.name = "checkout_started", setTimeout(() => s(i), 500)
            }), e.subscribe("er_events", i => {
                c = JSON.parse(JSON.stringify(i.customData)), i.name = "events", setTimeout(() => s(i), 500)
            }), e.subscribe("checkout_started", i => {
                setTimeout(() => s(i), 500)
            }), e.subscribe("checkout_completed", i => {
                setTimeout(() => {
                    s(i), a = Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15), console.log("er_client_id change", a), o.localStorage.setItem("er_client_id", a)
                }, 500)
            })
        }))
    });
    var V = j(v());
})();