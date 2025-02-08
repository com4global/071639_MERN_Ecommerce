namespace ApiOnLamda.Model
{
    public class checkoutrequestdata
    {

        
        
            /// <summary>
            /// The ID of the user placing the order.
            /// </summary>
            public int UserId { get; set; }
            public List<Cart> CartItems { get; set; }

        /// <summary>
        /// The total amount for the order.
        /// </summary>
          public decimal TotalAmount { get; set; }

            /// <summary>
            /// The method of delivery, e.g., "Ship" or "Pickup."
            /// </summary>
            public string DeliveryMethod { get; set; }

            /// <summary>
            /// The first line of the shipping address.
            /// </summary>
            public string AddressLine1 { get; set; }

            /// <summary>
            /// The second line of the shipping address (optional).
            /// </summary>
            public string AddressLine2 { get; set; }

            /// <summary>
            /// The city for the shipping address.
            /// </summary>
            public string City { get; set; }

            /// <summary>
            /// The state for the shipping address.
            /// </summary>
            public string State { get; set; }

            /// <summary>
            /// The postal code for the shipping address.
            /// </summary>
            public string PostalCode { get; set; }

            /// <summary>
            /// The country for the shipping address.
            /// </summary>
            public string Country { get; set; }

            /// <summary>
            /// The phone number associated with the order.
            /// </summary>
            public string Phone { get; set; }

            /// <summary>
            /// Indicates whether the user consents to marketing communication.
            /// </summary>
            public bool MarketingConsent { get; set; }
        

    }
}
