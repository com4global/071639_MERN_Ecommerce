! function(e) {
    "use strict";
    const n = {
            TRACKING_ACCEPTED: "trackingConsentAccepted",
            TRACKING_DECLINED: "trackingConsentDeclined",
            MARKETING_ACCEPTED: "firstPartyMarketingConsentAccepted",
            SALE_OF_DATA_ACCEPTED: "thirdPartyMarketingConsentAccepted",
            ANALYTICS_ACCEPTED: "analyticsConsentAccepted",
            PREFERENCES_ACCEPTED: "preferencesConsentAccepted",
            MARKETING_DECLINED: "firstPartyMarketingConsentDeclined",
            SALE_OF_DATA_DECLINED: "thirdPartyMarketingConsentDeclined",
            ANALYTICS_DECLINED: "analyticsConsentDeclined",
            PREFERENCES_DECLINED: "preferencesConsentDeclined",
            CONSENT_COLLECTED: "visitorConsentCollected",
            CONSENT_TRACKING_API_LOADED: "consentTrackingApiLoaded"
        },
        t = "2.1",
        o = {
            ACCEPTED: "yes",
            DECLINED: "no",
            NO_INTERACTION: "no_interaction",
            NO_VALUE: ""
        },
        r = {
            NO_VALUE: "",
            ACCEPTED: "1",
            DECLINED: "0"
        },
        i = {
            PREFERENCES: "p",
            ANALYTICS: "a",
            MARKETING: "m",
            SALE_OF_DATA: "t"
        },
        c = {
            MARKETING: "m",
            ANALYTICS: "a",
            PREFERENCES: "p",
            SALE_OF_DATA: "s"
        },
        a = {
            MARKETING: "marketing",
            ANALYTICS: "analytics",
            PREFERENCES: "preferences",
            SALE_OF_DATA: "sale_of_data",
            EMAIL: "email"
        },
        s = {
            HEADLESS_STOREFRONT: "headlessStorefront",
            ROOT_DOMAIN: "rootDomain",
            CHECKOUT_ROOT_DOMAIN: "checkoutRootDomain",
            STOREFRONT_ROOT_DOMAIN: "storefrontRootDomain",
            STOREFRONT_ACCESS_TOKEN: "storefrontAccessToken",
            IS_EXTENSION_TOKEN: "isExtensionToken",
            METAFIELDS: "metafields"
        };

    function u(e, n) {
        var t = Object.keys(e);
        if (Object.getOwnPropertySymbols) {
            var o = Object.getOwnPropertySymbols(e);
            n && (o = o.filter((function(n) {
                return Object.getOwnPropertyDescriptor(e, n).enumerable
            }))), t.push.apply(t, o)
        }
        return t
    }

    function l(e) {
        for (var n = 1; n < arguments.length; n++) {
            var t = null != arguments[n] ? arguments[n] : {};
            n % 2 ? u(Object(t), !0).forEach((function(n) {
                E(e, n, t[n])
            })) : Object.getOwnPropertyDescriptors ? Object.defineProperties(e, Object.getOwnPropertyDescriptors(t)) : u(Object(t)).forEach((function(n) {
                Object.defineProperty(e, n, Object.getOwnPropertyDescriptor(t, n))
            }))
        }
        return e
    }

    function E(e, n, t) {
        return n in e ? Object.defineProperty(e, n, {
            value: t,
            enumerable: !0,
            configurable: !0,
            writable: !0
        }) : e[n] = t, e
    }
    class d {}
    d.warn = e => {
        console.warn(e)
    }, d.error = e => {
        console.error(e)
    }, d.info = e => {
        console.info(e)
    }, d.debug = e => {
        console.debug(e)
    }, d.trace = e => {
        console.trace(e)
    };
    const f = d;

    function A(e) {
        let n = arguments.length > 1 && void 0 !== arguments[1] && arguments[1];
        const t = document.cookie ? document.cookie.split("; ") : [];
        for (let n = 0; n < t.length; n++) {
            const [o, r] = t[n].split("=");
            if (e === decodeURIComponent(o)) {
                return JSON.parse(decodeURIComponent(r))
            }
        }
        if (n && "_tracking_consent" === e && !window.localStorage.getItem("tracking_consent_fetched")) return console.debug("_tracking_consent missing"),
            function() {
                let e = arguments.length > 0 && void 0 !== arguments[0] ? arguments[0] : "/";
                const n = new XMLHttpRequest;
                n.open("HEAD", e, !1), n.withCredentials = !0, n.send()
            }(), window.localStorage.setItem("tracking_consent_fetched", "true"), A(e, !1)
    }

    function C(e) {
        return e === encodeURIComponent(decodeURIComponent(e))
    }

    function p(e, n, t, o) {
        if (!C(o)) throw new TypeError("Cookie value is not correctly URI encoded.");
        if (!C(e)) throw new TypeError("Cookie name is not correctly URI encoded.");
        let r = "".concat(e, "=").concat(o);
        r += "; path=/", n && (r += "; domain=".concat(n)), r += "; expires=".concat(new Date((new Date).getTime() + t).toUTCString()), document.cookie = r
    }
    const T = "_tracking_consent",
        g = 31536e6;

    function N() {
        const e = A(T);
        if (void 0 !== e && ! function(e) {
                var n;
                if (e.v !== t) return !0;
                if (null == e || null === (n = e.con) || void 0 === n || !n.CMP) return !0;
                return !1
            }(e)) return e
    }

    function _() {
        try {
            let e = N();
            if (!e) return;
            return e
        } catch (e) {
            return
        }
    }

    function D() {
        return {
            m: R(c.MARKETING),
            a: R(c.ANALYTICS),
            p: R(c.PREFERENCES),
            s: R(c.SALE_OF_DATA)
        }
    }

    function y() {
        return D()[c.SALE_OF_DATA]
    }

    function S() {
        let e = arguments.length > 0 && void 0 !== arguments[0] ? arguments[0] : null;
        return null === e && (e = _()), void 0 === e
    }

    function h(e) {
        switch (e) {
            case r.ACCEPTED:
                return o.ACCEPTED;
            case r.DECLINED:
                return o.DECLINED;
            default:
                return o.NO_VALUE
        }
    }

    function O(e) {
        switch (e) {
            case c.ANALYTICS:
                return a.ANALYTICS;
            case c.MARKETING:
                return a.MARKETING;
            case c.PREFERENCES:
                return a.PREFERENCES;
            case c.SALE_OF_DATA:
                return a.SALE_OF_DATA
        }
    }

    function R(e) {
        const n = _();
        if (!n) return r.NO_VALUE;
        const t = n.con.CMP;
        return t ? t[e] : r.NO_VALUE
    }

    function w() {
        const e = _();
        return S(e) ? "" : e.region || ""
    }

    function I(e) {
        const n = N();
        if (!n || !n.purposes) return !0;
        const t = n.purposes[e];
        return "boolean" != typeof t || t
    }

    function m() {
        return I(i.PREFERENCES)
    }

    function P() {
        return I(i.ANALYTICS)
    }

    function L() {
        return I(i.MARKETING)
    }

    function v() {
        return I(i.SALE_OF_DATA)
    }

    function k() {
        const e = N();
        return !!e && ("boolean" == typeof e.display_banner && e.display_banner)
    }

    function b() {
        const e = N();
        return e && e.sale_of_data_region || !1
    }

    function M(e) {
        void 0 !== e.granular_consent && function(e) {
            const t = e[i.MARKETING],
                o = e[i.SALE_OF_DATA],
                r = e[i.ANALYTICS],
                c = e[i.PREFERENCES];
            !0 === t ? F(n.MARKETING_ACCEPTED) : !1 === t && F(n.MARKETING_DECLINED);
            !0 === o ? F(n.SALE_OF_DATA_ACCEPTED) : !1 === o && F(n.SALE_OF_DATA_DECLINED);
            !0 === r ? F(n.ANALYTICS_ACCEPTED) : !1 === r && F(n.ANALYTICS_DECLINED);
            !0 === c ? F(n.PREFERENCES_ACCEPTED) : !1 === c && F(n.PREFERENCES_DECLINED);
            const a = function(e) {
                const n = {
                    marketingAllowed: e[i.MARKETING],
                    saleOfDataAllowed: e[i.SALE_OF_DATA],
                    analyticsAllowed: e[i.ANALYTICS],
                    preferencesAllowed: e[i.PREFERENCES],
                    firstPartyMarketingAllowed: e[i.MARKETING],
                    thirdPartyMarketingAllowed: e[i.SALE_OF_DATA]
                };
                return n
            }(e);
            F(n.CONSENT_COLLECTED, a);
            const s = [r, c, t, o];
            s.every((e => !0 === e)) && F(n.TRACKING_ACCEPTED);
            s.every((e => !1 === e)) && F(n.TRACKING_DECLINED)
        }({
            [i.PREFERENCES]: m(),
            [i.ANALYTICS]: P(),
            [i.MARKETING]: L(),
            [i.SALE_OF_DATA]: v()
        })
    }

    function F(e, n) {
        document.dispatchEvent(new CustomEvent(e, {
            detail: n || {}
        }))
    }

    function K(e, n) {
        if (null === e) return "null";
        if (Array.isArray(e)) {
            const n = e.map((e => K(e, !0))).join(",");
            return "[".concat(n, "]")
        }
        if ("object" == typeof e) {
            let t = [];
            for (const n in e) e.hasOwnProperty(n) && void 0 !== e[n] && t.push("".concat(n, ":").concat(K(e[n], !0)));
            const o = t.join(",");
            return n ? "{".concat(o, "}") : o
        }
        return "string" == typeof e ? '"'.concat(e, '"') : "".concat(e)
    }
    const G = "_landing_page",
        j = "_orig_referrer";

    function U(e) {
        const n = e.granular_consent,
            t = K(l(l({
                visitorConsent: l({
                    marketing: n.marketing,
                    analytics: n.analytics,
                    preferences: n.preferences,
                    saleOfData: n.sale_of_data
                }, n.metafields && {
                    metafields: n.metafields
                })
            }, n.email && {
                visitorEmail: n.email
            }), {}, {
                origReferrer: e.referrer,
                landingPage: e.landing_page
            }));
        return {
            query: "query { consentManagement { cookies(".concat(t, ") { trackingConsentCookie cookieDomain landingPageCookie origReferrerCookie } } }"),
            variables: {}
        }
    }

    function Y(e, n) {
        const t = e.granular_consent,
            o = t.storefrontAccessToken || function() {
                const e = document.documentElement.querySelector("#shopify-features"),
                    n = "Could not find liquid access token";
                if (!e) return void f.warn(n);
                const t = JSON.parse(e.textContent || "").accessToken;
                if (!t) return void f.warn(n);
                return t
            }(),
            r = t.checkoutRootDomain || window.location.host,
            i = t.isExtensionToken ? "Shopify-Storefront-Extension-Token" : "x-shopify-storefront-access-token",
            c = {
                headers: {
                    "content-type": "application/json",
                    [i]: o
                },
                body: JSON.stringify(U(e)),
                method: "POST"
            };
        return fetch("https://".concat(r, "/api/unstable/graphql.json"), c).then((e => {
            if (e.ok) return e.json();
            throw new Error("Server error")
        })).then((o => {
            const r = 31536e6,
                i = 12096e5,
                c = o.data.consentManagement.cookies.cookieDomain,
                a = c || t.checkoutRootDomain || window.location.hostname,
                s = t.storefrontRootDomain || c || window.location.hostname,
                u = o.data.consentManagement.cookies.trackingConsentCookie,
                l = o.data.consentManagement.cookies.landingPageCookie,
                E = o.data.consentManagement.cookies.origReferrerCookie;
            return p(T, a, r, u), l && E && (p(G, a, i, l), p(j, a, i, E)), s !== a && (p(T, s, r, u), l && E && (p(G, s, i, l), p(j, s, i, E))), M(e), void 0 !== n && n(null, o), o
        })).catch((e => {
            const t = "Error while setting storefront API consent: " + e.message;
            if (void 0 === n) throw {
                error: t
            };
            n({
                error: t
            })
        }))
    }

    function B(e, n) {
        if (function(e) {
                if ("boolean" != typeof e && "object" != typeof e) throw TypeError("setTrackingConsent must be called with a boolean or object consent value");
                if ("object" == typeof e) {
                    const n = Object.keys(e);
                    if (0 === n.length) throw TypeError("The submitted consent object is empty.");
                    const t = [a.MARKETING, a.ANALYTICS, a.PREFERENCES, a.SALE_OF_DATA, a.EMAIL, s.ROOT_DOMAIN, s.CHECKOUT_ROOT_DOMAIN, s.STOREFRONT_ROOT_DOMAIN, s.STOREFRONT_ACCESS_TOKEN, s.HEADLESS_STOREFRONT, s.IS_EXTENSION_TOKEN, s.METAFIELDS];
                    for (const e of n)
                        if (!t.includes(e)) throw TypeError("The submitted consent object should only contain the following keys: ".concat(t.join(", "), ". Extraneous key: ").concat(e, "."))
                }
            }(e), void 0 !== n && "function" != typeof n) throw TypeError("setTrackingConsent must be called with a callback function if the callback argument is provided");
        let t;
        if (!0 === e || !1 === e) {
            f.warn("Binary consent is deprecated. Please update to granular consent (shopify.dev/docs/api/consent-tracking)");
            t = {
                analytics: e,
                preferences: e,
                marketing: e
            }
        } else t = e;
        const o = function(e) {
                if (!e) return null;
                return X() ? document.referrer : ""
            }(t.analytics),
            r = function(e) {
                if (!e) return null;
                return X() ? window.location.pathname + window.location.search : "/"
            }(t.analytics);
        return Y(l(l({
            granular_consent: t
        }, null !== o && {
            referrer: o
        }), null !== r && {
            landing_page: r
        }), n)
    }

    function V(e, n) {
        if (f.warn("This method is deprecated. Please read shopify.dev/docs/api/customer-privacy for the latest information."), "boolean" != typeof e) throw TypeError("setCCPAConsent must be called with a boolean consent value");
        if ("function" != typeof n) throw TypeError("setCCPAConsent must be called with a callback function");
        return Y({
            granular_consent: {
                sale_of_data: e
            }
        }, n)
    }

    function x() {
        if (S()) return o.NO_VALUE;
        const e = D();
        return e[c.MARKETING] === r.ACCEPTED && e[c.ANALYTICS] === r.ACCEPTED ? o.ACCEPTED : e[c.MARKETING] === r.DECLINED || e[c.ANALYTICS] === r.DECLINED ? o.DECLINED : o.NO_INTERACTION
    }

    function H() {
        f.warn("getRegulation is deprecated and will be removed.");
        const e = w();
        return "" === e ? "" : ["AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IS", "IE", "IT", "LV", "LI", "LT", "LU", "MT", "NL", "NO", "PL", "PT", "RO", "SI", "SK", "ES", "SE", "GB"].includes(e.slice(0, 2)) ? "GDPR" : "US" === e.slice(0, 2) && ["CA", "VA"].includes(e.slice(2, 4)) ? "CCPA" : ""
    }

    function q() {
        return f.warn("getShopPrefs is deprecated and will be removed."), {
            limit: []
        }
    }

    function J() {
        return w()
    }

    function X() {
        if ("" === document.referrer) return !0;
        const e = document.createElement("a");
        return e.href = document.referrer, window.location.hostname != e.hostname
    }

    function $() {
        return f.warn("isRegulationEnforced is deprecated and will be removed."), !0
    }

    function Z() {
        return !!S() || L() && P()
    }

    function z() {
        return b() ? "string" == typeof navigator.globalPrivacyControl ? "1" !== navigator.globalPrivacyControl : "boolean" == typeof navigator.globalPrivacyControl ? !navigator.globalPrivacyControl : null : null
    }

    function Q() {
        return f.warn("userDataCanBeSold is deprecated and will be replaced with saleOfDataAllowed."), v()
    }

    function W() {
        return k() && x() === o.NO_INTERACTION
    }

    function ee() {
        return !1 === z() ? o.DECLINED : (e = y(), S() ? o.NO_VALUE : e === r.NO_VALUE ? o.NO_INTERACTION : h(e));
        var e
    }

    function ne() {
        return f.warn("shouldShowCCPABanner is deprecated and will be removed."), b() && ee() === o.NO_INTERACTION
    }

    function te() {
        return !0
    }

    function oe(e) {
        return function(e) {
            const n = _();
            if (S(n) || !n.cus) return;
            const t = n.cus[encodeURIComponent(e)];
            return t ? decodeURIComponent(t) : t
        }(e)
    }
    const re = "95ba910bcec4542ef2a0b64cd7ca666c";

    function ie(e, n, t) {
        try {
            var o;
            ! function(e) {
                const n = new XMLHttpRequest;
                n.open("POST", "https://notify.bugsnag.com/", !0), n.setRequestHeader("Content-Type", "application/json"), n.setRequestHeader("Bugsnag-Api-Key", re), n.setRequestHeader("Bugsnag-Payload-Version", "5");
                const t = function(e) {
                    const n = function(e) {
                            return e.stackTrace || e.stack || e.description || e.name
                        }(e.error),
                        [t, o] = (n || "unknown error").split("\n")[0].split(":");
                    return JSON.stringify({
                        payloadVersion: 5,
                        notifier: {
                            name: "ConsentTrackingAPI",
                            version: "latest",
                            url: "-"
                        },
                        events: [{
                            exceptions: [{
                                errorClass: (t || "").trim(),
                                message: (o || "").trim(),
                                stacktrace: [{
                                    file: "consent-tracking-api.js",
                                    lineNumber: "1",
                                    method: n
                                }],
                                type: "browserjs"
                            }],
                            context: e.context || "general",
                            app: {
                                id: "ConsentTrackingAPI",
                                version: "latest"
                            },
                            metaData: {
                                request: {
                                    shopId: e.shopId,
                                    shopUrl: window.location.href
                                },
                                device: {
                                    userAgent: window.navigator.userAgent
                                },
                                "Additional Notes": e.notes
                            },
                            unhandled: !1
                        }]
                    })
                }(e);
                n.send(t)
            }({
                error: e,
                context: n,
                shopId: ae() || (null === (o = window.Shopify) || void 0 === o ? void 0 : o.shop),
                notes: t
            })
        } catch (e) {}
    }

    function ce(e) {
        return function() {
            try {
                return e(...arguments)
            } catch (e) {
                throw e instanceof TypeError || ie(e), e
            }
        }
    }

    function ae() {
        try {
            const e = document.getElementById("shopify-features").textContent;
            return JSON.parse(e).shopId
        } catch (e) {
            return null
        }
    }

    function se() {
        return L()
    }

    function ue() {
        return v()
    }

    function le() {
        const e = {},
            n = D();
        for (const t of Object.keys(n)) e[O(t)] = h(n[t]);
        return e
    }

    function Ee(e, n) {
        return "object" == typeof e && e.headlessStorefront && !e.storefrontAccessToken ? (f.warn("Headless consent has been updated. Please read shopify.dev/docs/api/customer-privacy to integrate."), function(e, n) {
            function o(e) {
                let n = arguments.length > 1 && void 0 !== arguments[1] ? arguments[1] : r.NO_VALUE;
                return !0 === e ? r.ACCEPTED : !1 === e ? r.DECLINED : n
            }
            const i = {
                    [c.ANALYTICS]: o(e[a.ANALYTICS], r.DECLINED),
                    [c.MARKETING]: o(e[a.MARKETING], r.DECLINED),
                    [c.PREFERENCES]: o(e[a.PREFERENCES], r.DECLINED),
                    [c.SALE_OF_DATA]: o(e[a.SALE_OF_DATA])
                },
                s = {
                    v: t,
                    reg: "",
                    con: {
                        CMP: i
                    }
                },
                u = encodeURIComponent(JSON.stringify(s));
            return p(T, e.rootDomain, g, u), n(null), new Promise(((e, n) => {}))
        }(e, n || (() => {}))) : B(e, n)
    }
    const de = e => {
        let {
            useBugsnagReporting: n
        } = e;
        y() != r.DECLINED && !1 === z() && V(!1, (() => !1));
        const t = {
            getTrackingConsent: x,
            setTrackingConsent: Ee,
            userCanBeTracked: Z,
            getRegulation: H,
            isRegulationEnforced: $,
            getShopPrefs: q,
            shouldShowGDPRBanner: W,
            userDataCanBeSold: Q,
            setCCPAConsent: V,
            getCCPAConsent: ee,
            shouldShowCCPABanner: ne,
            doesMerchantSupportGranularConsent: te,
            analyticsProcessingAllowed: P,
            preferencesProcessingAllowed: m,
            marketingAllowed: se,
            firstPartyMarketingAllowed: se,
            saleOfDataAllowed: ue,
            thirdPartyMarketingAllowed: ue,
            currentVisitorConsent: le,
            shouldShowBanner: k,
            saleOfDataRegion: b,
            getRegion: J,
            getTrackingConsentMetafield: oe,
            unstable: {
                analyticsProcessingAllowed: P,
                preferencesProcessingAllowed: m,
                marketingAllowed: se,
                saleOfDataAllowed: ue,
                currentVisitorConsent: le,
                shouldShowBanner: k,
                saleOfDataRegion: b
            },
            __metadata__: {
                name: "@shopify/consent-tracking-api",
                version: "v0.1",
                description: "Shopify Consent Tracking API"
            }
        };
        if (!n) return t;
        const o = ["unstable"];
        for (const e in t) t.hasOwnProperty(e) && (t[e] = o.includes(e) ? t[e] : ce(t[e]));
        return t
    };

    function fe() {
        return de(arguments.length > 0 && void 0 !== arguments[0] ? arguments[0] : {
            useBugsnagReporting: !1
        })
    }

    function Ae() {
        var e, t;
        const o = fe({
            useBugsnagReporting: !0
        });
        if (window.Shopify.trackingConsent || window.Shopify.customerPrivacy) {
            const n = null === (e = window.Shopify.customerPrivacy.__metadata__) || void 0 === e ? void 0 : e.version,
                r = null === (t = o.__metadata__) || void 0 === t ? void 0 : t.version,
                i = `Multiple versions of Shopify.trackingConsent or Shopify.customerPrivacy loaded -  Version '${n}' is already loaded but replacing with version '${r}'.\n\nThis could result in unexpected behavior. See documentation https://shopify.dev/docs/api/customer-privacy for more information.`,
                c = "Shopify.trackingConsent or Shopify.customerPrivacy already exists.\n\nLoading multiple versions could result in unexpected behavior. See documentation https://shopify.dev/docs/api/customer-privacy for more information.";
            console.warn(n && r ? i : c)
        }
        window.Shopify.customerPrivacy = window.Shopify.trackingConsent = o, F(n.CONSENT_TRACKING_API_LOADED)
    }
    window.Shopify = window.Shopify ? window.Shopify : {}, Ae(), e.default = fe, e.setGlobalObject = Ae, Object.defineProperty(e, "__esModule", {
        value: !0
    })
}({});
//# sourceMappingURL=consent-tracking-api.js.map