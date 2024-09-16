This project is part of a suite of synchronization solutions designed to keep data consistent between various data sources and a data warehouse.
Specifically, this solution synchronizes offer transaction data from a CRM system to a data warehouse.

## Overview
Offer Transaction Sync is a .NET 8 worker service that periodically fetches offer transaction data from a CRM system, compares it with existing data in the data warehouse, and performs necessary insert, update, or delete operations to ensure data consistency.

## Features

• Automated synchronization of offer transaction data

• Configurable job scheduling

• Efficient handling of inserts, updates, and deletes

• Error logging and monitoring

• Reflection-based entity mapping for easy maintenance and extensibility
