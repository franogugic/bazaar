CREATE TABLE LISTINGS(
    id                  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, 
    seller_id           INT NOT NULL,
    title               VARCHAR(100) NOT NULL,
    description         VARCHAR(5000) NOT NULL,
    price               NUMERIC(12,2) NOT NULL,
    quantity            INT NOT NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT listings_seller_fk       FOREIGN KEY (seller_id) REFERENCES users (id),
    CONSTRAINT listings_price_positive  CHECK (price > 0),
    CONSTRAINT listings_quantity_valid  CHECK (quantity >= 0)
);

CREATE INDEX listings_seller_idx ON listings (seller_id);