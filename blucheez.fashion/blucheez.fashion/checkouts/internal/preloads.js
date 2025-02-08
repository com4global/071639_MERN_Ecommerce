(function() {
    var baseURL = "https://cdn.shopify.com/shopifycloud/checkout-web/assets/";
    var scripts = ["https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/polyfills.CqZeYl46.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/app.DQoNRaWY.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/VaultedContact.BpzQYeie.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/DeliveryMethodSelectorSection.DqxnPt7o.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/useUnauthenticatedErrorModal.BFSIcMRk.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/AmazonPayPCIButton.DomwFF9Z.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/CheckoutAsGuest.DqQHe0Ru.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/useRefEffect.BOInSGCP.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/ShopPayLogo.BU5TFr_-.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/PickupPointCarrierLogo.2n6nkPBi.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/hooks.mS2UnkQc.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/LocalizationExtensionField.bH0Tbo7N.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/useShowShopPayOptin.CoMcgyeE.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/Rollup.CnlOAYQl.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/useShopPayRequiresVerification.FQdkGvH9.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/Section.BArcSAAQ.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/GooglePayPCIButton.DD8qnQ5M.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/PayButtonSection.CbEqU_ZC.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/RageClickCapture.BXy8nX41.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/useInstallmentsErrorHandler.Ijt3TM7_.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/index.DyncG_VF.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/DutyOptions.CbfXwlEz.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/SubscriptionPriceBreakdown.CtTACiVg.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/useAmazonContact.DHmi1ieD.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/StockProblemsLineItemList.BvFRKo3p.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/useGetBuyWithPrimeCheckoutSessionId.BkSikJ53.js", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/index.Chb8ohbM.js"];
    var styles = ["https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/app.DUpjA8nd.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/VaultedContact.BsDM6oHQ.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/DeliveryMethodSelectorSection.DNerkzQV.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/CheckoutAsGuest.CUoq2pCx.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/ShopPayLogo.D_HPU8Dh.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/PickupPointCarrierLogo.C0wRU6wV.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/LocalizationExtensionField.BO3829nT.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/Rollup.o9Mx-fKL.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/Section.BzDw6wmZ.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/PayButtonSection.DF7trkKf.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/RageClickCapture.DnkQ4tsk.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/DutyOptions.Bd1Z60K2.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/SubscriptionPriceBreakdown.Bqs0s4oM.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/useAmazonContact.D-Ox6Dnf.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/StockProblemsLineItemList.CxdIQKjw.css", "https://cdn.shopify.com/shopifycloud/checkout-web/assets/c1.en/assets/useGetBuyWithPrimeCheckoutSessionId.DVQdwG9J.css"];
    var fontPreconnectUrls = ["https://fonts.shopifycdn.com"];
    var fontPrefetchUrls = ["https://fonts.shopifycdn.com/lato/lato_n4.c86cddcf8b15d564761aaa71b6201ea326f3648b.woff2?h1=Ymx1Y2hlZXouZmFzaGlvbg&hmac=30287edab2de06571a2ca96eeec92033b88b734b43f63d90fed6f1fb1e4951cd", "https://fonts.shopifycdn.com/lato/lato_n7.f0037142450bd729bdf6ba826f5fdcd80f2787ba.woff2?h1=Ymx1Y2hlZXouZmFzaGlvbg&hmac=c26ef4c32b6f1654fbe8137844193b5487e81ab068c05b55adb325d3dcc5e3b8"];
    var imgPrefetchUrls = ["https://cdn.shopify.com/s/files/1/0585/0077/6131/files/LOGO-01_x320.svg?v=1693036316"];

    function preconnect(url, callback) {
        var link = document.createElement('link');
        link.rel = 'dns-prefetch preconnect';
        link.href = url;
        link.crossOrigin = '';
        link.onload = link.onerror = callback;
        document.head.appendChild(link);
    }

    function preconnectAssets() {
        var resources = [baseURL].concat(fontPreconnectUrls);
        var index = 0;
        (function next() {
            var res = resources[index++];
            if (res) preconnect(res, next);
        })();
    }

    function prefetch(url, as, callback) {
        var link = document.createElement('link');
        if (link.relList.supports('prefetch')) {
            link.rel = 'prefetch';
            link.fetchPriority = 'low';
            link.as = as;
            if (as === 'font') link.type = 'font/woff2';
            link.href = url;
            link.crossOrigin = '';
            link.onload = link.onerror = callback;
            document.head.appendChild(link);
        } else {
            var xhr = new XMLHttpRequest();
            xhr.open('GET', url, true);
            xhr.onloadend = callback;
            xhr.send();
        }
    }

    function prefetchAssets() {
        var resources = [].concat(
            scripts.map(function(url) {
                return [url, 'script'];
            }),
            styles.map(function(url) {
                return [url, 'style'];
            }),
            fontPrefetchUrls.map(function(url) {
                return [url, 'font'];
            }),
            imgPrefetchUrls.map(function(url) {
                return [url, 'image'];
            })
        );
        var index = 0;

        function run() {
            var res = resources[index++];
            if (res) prefetch(res[0], res[1], next);
        }
        var next = (self.requestIdleCallback || setTimeout).bind(self, run);
        next();
    }

    function onLoaded() {
        try {
            if (parseFloat(navigator.connection.effectiveType) > 2 && !navigator.connection.saveData) {
                preconnectAssets();
                prefetchAssets();
            }
        } catch (e) {}
    }

    if (document.readyState === 'complete') {
        onLoaded();
    } else {
        addEventListener('load', onLoaded);
    }
})();