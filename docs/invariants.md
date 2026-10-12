# Bazaar: Invariants

An invariant is a rule that must always be true, even after a crash or when many users act at the same time. Each invariant below has a statement and a check. Every check can be run automatically with a query or a script.

## 1. Unique accounts

**Always true:** no two accounts have the same username or email.

**Check:** for each field, group the accounts by its value and look for groups with more than one account. The result must be zero rows.

## 2. Inventory is never negative

**Always true:** the quantity of a listing in stock is always `>= 0`.

**Check:** a query that returns all listings with `quantity < 0`. The result must be zero rows.

## 3. No double charge

**Always true:** one checkout request causes at most one payment.

**Check:** group the payments by `idempotency_key` and look for groups with more than one payment. The result must be zero rows.

## 4. Money and order go together

**Always true:** money is taken from a buyer if and only if the order exists.

**Check:** two queries. The first finds orders without a payment. The second finds payments without an order. Both must return zero rows.

## 5. Wallet balance is never negative

**Always true:** the balance of a wallet is always `>= 0`.

**Check:** a query that returns all wallets with `balance < 0`. The result must be zero rows.

## 6. Ledger is balanced

**Always true:** the sum of all ledger entries is always zero.

**Check:** sum the `amount` column of the whole ledger. The result must be 0.

## 7. Copies match the source

**Always true:** after enough time without writes, every derived store (search, feed, cache) matches the database.

**Check:** take a sample of listings and compute a checksum from the ID and version, in the database and in the derived store. Compare them. Skip data that changed in the last few minutes.

## 8. Confirmed writes are not lost

**Always true:** if Bazaar confirmed an order or a payment, it still exists after a crash of one server.

**Check:** the client keeps a history of every request and response. After the crash, every confirmed request from the history must exist in the database.


