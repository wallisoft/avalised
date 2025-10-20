# Payment Setup Guide

## For Customers

### How to Purchase a Commercial License

1. **Email us:** wallisoft@gmail.com
2. **Subject:** "Visualised Commercial License - [Your Company]"
3. **Include:**
   - Company name
   - Number of employees
   - Billing address
   - VAT number (if applicable)
   - Preferred tier (Startup/Growth/Enterprise)

We'll send you:
- Invoice
- Payment instructions
- License certificate
- Support access details

### Payment Methods

- **Bank Transfer** (preferred for UK/EU)
- **PayPal** (instant, small fee)
- **Stripe** (credit card, instant)

---

## For Steve: Setting Up Payments

### Option 1: Stripe (RECOMMENDED)

**Why Stripe:**
- Professional invoicing
- Automatic receipts
- Recurring billing
- Low fees (1.5% + 20p for UK cards)
- VAT handling built-in

**Setup Steps:**

1. **Create account:** https://stripe.com/gb
2. **Verify business details**
3. **Create Products:**
   - Visualised Startup (£500/year)
   - Visualised Growth (£2,500/year)
   - Visualised Enterprise (£10,000/year)
4. **Enable:**
   - Invoicing
   - Customer portal
   - Email receipts
5. **Add payment link to website**

**Payment Link Example:**
```
https://buy.stripe.com/YOUR_LINK_HERE
```

---

### Option 2: PayPal Business

**Setup:**
1. Upgrade to PayPal Business
2. Create Invoice templates
3. Enable recurring payments
4. Add email: wallisoft@gmail.com

**Fees:** 2.9% + 30p per transaction

---

### Option 3: Bank Transfer (Manual)

**Your Details:**
- Account name: Steve Wallis / Wallisoft
- Sort code: [YOUR SORT CODE]
- Account number: [YOUR ACCOUNT]
- Reference: "Visualised-[Company]"

**Process:**
1. Customer emails for license
2. Send invoice (PDF)
3. Wait for bank transfer
4. Send license certificate
5. Add to spreadsheet for tracking

---

### Recommended: Stripe + Bank Transfer

- **Stripe:** For international customers, credit cards
- **Bank Transfer:** For UK customers, large invoices

---

### Invoice Template

Save as `invoice-template.html`:
```html
<!DOCTYPE html>
<html>
<head>
    <title>Invoice</title>
    <style>
        body { font-family: Arial; max-width: 800px; margin: 40px auto; }
        .header { border-bottom: 2px solid #667eea; padding-bottom: 20px; }
        .invoice-details { margin: 30px 0; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background: #667eea; color: white; }
        .total { font-size: 1.3em; font-weight: bold; text-align: right; }
    </style>
</head>
<body>
    <div class="header">
        <h1>🌳 VISUALISED</h1>
        <p>Steve Wallis / Wallisoft<br>
        Eastbourne, England<br>
        wallisoft@gmail.com</p>
    </div>

    <div class="invoice-details">
        <p><strong>Invoice Number:</strong> VIS-2025-001<br>
        <strong>Date:</strong> [DATE]<br>
        <strong>Due Date:</strong> [DUE DATE]</p>

        <p><strong>Bill To:</strong><br>
        [COMPANY NAME]<br>
        [ADDRESS]<br>
        [VAT NUMBER]</p>
    </div>

    <table>
        <tr>
            <th>Description</th>
            <th>Period</th>
            <th>Amount</th>
        </tr>
        <tr>
            <td>Visualised Commercial License - [TIER]</td>
            <td>12 months from [DATE]</td>
            <td>£[AMOUNT]</td>
        </tr>
        <tr>
            <td colspan="2" class="total">Total Due:</td>
            <td class="total">£[AMOUNT]</td>
        </tr>
    </table>

    <p><strong>Payment Methods:</strong></p>
    <ul>
        <li>Stripe: [PAYMENT LINK]</li>
        <li>Bank Transfer: [BANK DETAILS]</li>
        <li>PayPal: wallisoft@gmail.com</li>
    </ul>

    <p style="margin-top: 40px; font-size: 0.9em; color: #666;">
    Thank you for supporting Visualised!<br>
    Questions? Email wallisoft@gmail.com
    </p>
</body>
</html>
```

---

### Customer Tracking Spreadsheet

| Company | Tier | Start Date | Renewal | Status | Email | Notes |
|---------|------|-----------|---------|--------|-------|-------|
| Example Corp | Growth | 2025-10-20 | 2026-10-20 | Active | cto@example.com | Via Stripe |

---

### Email Templates

**Initial Response:**
```
Subject: Re: Visualised Commercial License

Hi [NAME],

Thanks for your interest in Visualised!

Based on [EMPLOYEES] employees, you need our [TIER] license (£[AMOUNT]/year).

This includes:
- [BENEFITS]

I'll send an invoice shortly. Payment via Stripe/Bank Transfer/PayPal.

Questions? Just ask!

Best,
Steve Wallis
wallisoft@gmail.com
```

**Invoice Sent:**
```
Subject: Invoice #VIS-2025-XXX - Visualised License

Hi [NAME],

Please find attached your invoice for Visualised Commercial License.

Payment link: [STRIPE LINK]
Or bank transfer: [DETAILS]

Once paid, I'll send:
- License certificate
- Support channel access
- Getting started guide

Thanks!
Steve
```

**License Activated:**
```
Subject: Welcome to Visualised! License Activated

Hi [NAME],

Your Visualised license is now active! 🎉

License details:
- Tier: [TIER]
- Valid until: [DATE]
- Support: wallisoft@gmail.com

Getting started:
1. [LINK TO DOCS]
2. [SUPPORT CHANNEL]

Questions? I'm here to help!

Steve
```
