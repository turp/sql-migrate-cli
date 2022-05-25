ALTER TABLE dbo.[StateProvinces]  WITH CHECK ADD  CONSTRAINT [FK_Application_StateProvinces_Application_People] FOREIGN KEY([LastEditedBy])
REFERENCES dbo.[People] ([PersonID])
GO

ALTER TABLE dbo.[StateProvinces] CHECK CONSTRAINT [FK_Application_StateProvinces_Application_People]
GO

ALTER TABLE dbo.[StateProvinces]  WITH CHECK ADD  CONSTRAINT [FK_Application_StateProvinces_CountryID_Application_Countries] FOREIGN KEY([CountryID])
REFERENCES dbo.[Countries] ([CountryID])
GO

ALTER TABLE dbo.[StateProvinces] CHECK CONSTRAINT [FK_Application_StateProvinces_CountryID_Application_Countries]
GO

ALTER TABLE dbo.[Countries]  WITH CHECK ADD  CONSTRAINT [FK_Application_Countries_Application_People] FOREIGN KEY([LastEditedBy])
REFERENCES dbo.[People] ([PersonID])
GO

ALTER TABLE dbo.[Countries] CHECK CONSTRAINT [FK_Application_Countries_Application_People]
GO

ALTER TABLE dbo.[DeliveryMethods]  WITH CHECK ADD  CONSTRAINT [FK_Application_DeliveryMethods_Application_People] FOREIGN KEY([LastEditedBy])
REFERENCES dbo.[People] ([PersonID])
GO

ALTER TABLE dbo.[DeliveryMethods] CHECK CONSTRAINT [FK_Application_DeliveryMethods_Application_People]
GO

ALTER TABLE dbo.[People]  WITH CHECK ADD  CONSTRAINT [FK_Application_People_Application_People] FOREIGN KEY([LastEditedBy])
REFERENCES dbo.[People] ([PersonID])
GO

ALTER TABLE dbo.[People] CHECK CONSTRAINT [FK_Application_People_Application_People]
GO

ALTER TABLE dbo.[Cities]  WITH CHECK ADD  CONSTRAINT [FK_Application_Cities_Application_People] FOREIGN KEY([LastEditedBy])
REFERENCES dbo.[People] ([PersonID])
GO

ALTER TABLE dbo.[Cities] CHECK CONSTRAINT [FK_Application_Cities_Application_People]
GO

ALTER TABLE dbo.[Cities]  WITH CHECK ADD  CONSTRAINT [FK_Application_Cities_StateProvinceID_Application_StateProvinces] FOREIGN KEY([StateProvinceID])
REFERENCES dbo.[StateProvinces] ([StateProvinceID])
GO

ALTER TABLE dbo.[Cities] CHECK CONSTRAINT [FK_Application_Cities_StateProvinceID_Application_StateProvinces]
GO

ALTER TABLE dbo.[TransactionTypes]  WITH CHECK ADD  CONSTRAINT [FK_Application_TransactionTypes_Application_People] FOREIGN KEY([LastEditedBy])
REFERENCES dbo.[People] ([PersonID])
GO

ALTER TABLE dbo.[TransactionTypes] CHECK CONSTRAINT [FK_Application_TransactionTypes_Application_People]
GO

ALTER TABLE dbo.[PaymentMethods]  WITH CHECK ADD  CONSTRAINT [FK_Application_PaymentMethods_Application_People] FOREIGN KEY([LastEditedBy])
REFERENCES dbo.[People] ([PersonID])
GO

ALTER TABLE dbo.[PaymentMethods] CHECK CONSTRAINT [FK_Application_PaymentMethods_Application_People]
GO
