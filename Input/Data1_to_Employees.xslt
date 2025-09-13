<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="xml" indent="yes" encoding="utf-8"/>
    
    <xsl:template match="/">
        <Employees>
            <xsl:for-each select="Pay/item[not(@name = preceding-sibling::item/@name and @surname = preceding-sibling::item/@surname)]">
                <xsl:variable name="currentName" select="@name"/>
                <xsl:variable name="currentSurname" select="@surname"/>
                
                <Employee name="{$currentName}" surname="{$currentSurname}">
                    <xsl:for-each select="/Pay/item[@name = $currentName and @surname = $currentSurname]">
                        <salary amount="{@amount}" mount="{@mount}"/>
                    </xsl:for-each>
                </Employee>
            </xsl:for-each>
        </Employees>
    </xsl:template>
</xsl:stylesheet>